using System.Text.Json;
using CashFlow.Application.Transactions;
using CashFlow.Infrastructure.Messaging.Kafka;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace CashFlow.Infrastructure.Messaging.Redis;

public sealed class RedisTransactionOutboxDispatcherHostedService : BackgroundService
{
    private readonly IConnectionMultiplexer _connectionMultiplexer;
    private readonly KafkaTransactionEventPublisher _kafkaPublisher;
    private readonly RedisOptions _options;
    private readonly ILogger<RedisTransactionOutboxDispatcherHostedService> _logger;

    public RedisTransactionOutboxDispatcherHostedService(
        IConnectionMultiplexer connectionMultiplexer,
        KafkaTransactionEventPublisher kafkaPublisher,
        IOptions<RedisOptions> options,
        ILogger<RedisTransactionOutboxDispatcherHostedService> logger)
    {
        _connectionMultiplexer = connectionMultiplexer;
        _kafkaPublisher = kafkaPublisher;
        _options = options.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var database = _connectionMultiplexer.GetDatabase();

        await RecoverProcessingMessagesAsync(database);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var payload = database.ListRightPopLeftPush(_options.OutboxPendingQueueKey, _options.OutboxProcessingQueueKey);

                if (payload.IsNullOrEmpty)
                {
                    await Task.Delay(_options.PollDelayMilliseconds, stoppingToken);
                    continue;
                }

                await DispatchAsync(database, payload, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while dispatching Redis outbox messages.");
                await Task.Delay(_options.RetryDelayMilliseconds, stoppingToken);
            }
        }
    }

    private Task RecoverProcessingMessagesAsync(IDatabase database)
    {
        while (database.ListLength(_options.OutboxProcessingQueueKey) > 0)
        {
            var payload = database.ListRightPopLeftPush(_options.OutboxProcessingQueueKey, _options.OutboxPendingQueueKey);

            if (payload.IsNullOrEmpty)
            {
                break;
            }
        }

        return Task.CompletedTask;
    }

    private async Task DispatchAsync(IDatabase database, RedisValue payload, CancellationToken cancellationToken)
    {
        try
        {
            var json = payload.ToString();
            var integrationEvent = JsonSerializer.Deserialize<TransactionCreatedIntegrationEvent>(json)
                ?? throw new InvalidOperationException("Invalid outbox payload.");

            await _kafkaPublisher.PublishAsync(integrationEvent, cancellationToken);
            database.ListRemove(_options.OutboxProcessingQueueKey, payload, 1);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Invalid outbox payload moved to Redis dead letter queue.");
            database.ListRemove(_options.OutboxProcessingQueueKey, payload, 1);
            database.ListLeftPush(_options.OutboxDeadLetterQueueKey, payload);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to publish outbox message to Kafka. Returning it to Redis pending queue.");
            database.ListRemove(_options.OutboxProcessingQueueKey, payload, 1);
            database.ListLeftPush(_options.OutboxPendingQueueKey, payload);
            await Task.Delay(_options.RetryDelayMilliseconds, cancellationToken);
        }
    }
}

