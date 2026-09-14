using System.Text.Json;
using CashFlow.Application.Transactions;
using CashFlow.Domain.Transactions;
using CashFlow.Infrastructure.Persistence;
using CashFlow.Infrastructure.Persistence.Entities;
using Confluent.Kafka;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CashFlow.Infrastructure.Messaging.Kafka;

public sealed class TransactionConsolidatorHostedService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly KafkaOptions _options;
    private readonly ILogger<TransactionConsolidatorHostedService> _logger;
    private static readonly TimeSpan RetryDelay = TimeSpan.FromSeconds(5);

    public TransactionConsolidatorHostedService(
        IServiceScopeFactory scopeFactory,
        IOptions<KafkaOptions> options,
        ILogger<TransactionConsolidatorHostedService> logger)
    {
        _scopeFactory = scopeFactory;
        _options = options.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Yield();

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await RunConsumerLoopAsync(stoppingToken);
                return;
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kafka consolidation loop failed. Retrying in {RetryDelay}.", RetryDelay);

                try
                {
                    await Task.Delay(RetryDelay, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }
        }
    }

    private async Task RunConsumerLoopAsync(CancellationToken stoppingToken)
    {
        var consumerConfig = new ConsumerConfig
        {
            BootstrapServers = _options.BootstrapServers,
            GroupId = _options.ConsumerGroupId,
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = false,
            AllowAutoCreateTopics = true
        };

        var producerConfig = new ProducerConfig
        {
            BootstrapServers = _options.BootstrapServers,
            Acks = Acks.All,
            EnableIdempotence = true,
            MessageSendMaxRetries = 5
        };

        using var consumer = new ConsumerBuilder<string, string>(consumerConfig).Build();
        using var dlqProducer = new ProducerBuilder<string, string>(producerConfig).Build();

        consumer.Subscribe(_options.TransactionCreatedTopic);

        while (!stoppingToken.IsCancellationRequested)
        {
            ConsumeResult<string, string>? consumeResult;

            try
            {
                consumeResult = consumer.Consume(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed while consuming Kafka messages.");
                continue;
            }

            try
            {
                var integrationEvent = JsonSerializer.Deserialize<TransactionCreatedIntegrationEvent>(consumeResult.Message.Value)
                    ?? throw new InvalidOperationException("Invalid transaction event payload.");

                await ConsolidateBalanceAsync(integrationEvent, stoppingToken);
                consumer.Commit(consumeResult);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to consolidate message. Sending to DLQ.");

                await SendToDlqAsync(dlqProducer, consumeResult, ex, stoppingToken);
                consumer.Commit(consumeResult);
            }
        }
    }

    private async Task SendToDlqAsync(
        IProducer<string, string> dlqProducer,
        ConsumeResult<string, string> consumeResult,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var dlqMessage = new DlqMessage(
            consumeResult.Message.Value,
            exception.ToString(),
            DateTime.UtcNow);

        var dlqPayload = JsonSerializer.Serialize(dlqMessage);

        await dlqProducer.ProduceAsync(
            _options.TransactionCreatedDlqTopic,
            new Message<string, string>
            {
                Key = consumeResult.Message.Key,
                Value = dlqPayload
            },
            cancellationToken);
    }

    private async Task ConsolidateBalanceAsync(TransactionCreatedIntegrationEvent integrationEvent, CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<TransactionType>(integrationEvent.Type, true, out var type))
        {
            throw new InvalidOperationException($"Unsupported transaction type: {integrationEvent.Type}");
        }

        var date = DateOnly.FromDateTime(integrationEvent.OccurredAtUtc);

        using var scope = _scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<CashFlowDbContext>();

        var balance = await dbContext.DailyBalances.FirstOrDefaultAsync(x => x.Date == date, cancellationToken);

        if (balance is null)
        {
            balance = new DailyBalanceEntity
            {
                Date = date,
                Credits = 0m,
                Debits = 0m,
                UpdatedAtUtc = DateTime.UtcNow
            };

            await dbContext.DailyBalances.AddAsync(balance, cancellationToken);
        }

        if (type == TransactionType.Credit)
        {
            balance.Credits += integrationEvent.Amount;
        }
        else
        {
            balance.Debits += integrationEvent.Amount;
        }

        balance.UpdatedAtUtc = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}

