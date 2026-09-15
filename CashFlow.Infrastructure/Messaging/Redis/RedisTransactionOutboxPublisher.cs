using System.Text.Json;
using CashFlow.Application.Transactions;
using StackExchange.Redis;
using Microsoft.Extensions.Options;

namespace CashFlow.Infrastructure.Messaging.Redis;

public sealed class RedisTransactionOutboxPublisher : ITransactionEventPublisher
{
    private readonly IDatabase _database;
    private readonly RedisOptions _options;

    public RedisTransactionOutboxPublisher(IConnectionMultiplexer connectionMultiplexer, IOptions<RedisOptions> options)
    {
        _database = connectionMultiplexer.GetDatabase();
        _options = options.Value;
    }

    public Task PublishAsync(TransactionCreatedIntegrationEvent integrationEvent, CancellationToken cancellationToken)
    {
        var payload = JsonSerializer.Serialize(integrationEvent);

        return PushWithRetryAsync(payload, cancellationToken);
    }

    private async Task PushWithRetryAsync(string payload, CancellationToken cancellationToken)
    {
        const int maxAttempts = 3;

        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                _database.ListLeftPush(_options.OutboxPendingQueueKey, payload);
                return;
            }
            catch when (attempt < maxAttempts)
            {
                await Task.Delay(TimeSpan.FromMilliseconds(200 * attempt), cancellationToken);
            }
        }

        _database.ListLeftPush(_options.OutboxPendingQueueKey, payload);

    }
}

