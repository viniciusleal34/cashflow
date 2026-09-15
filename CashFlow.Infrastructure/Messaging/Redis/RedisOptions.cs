namespace CashFlow.Infrastructure.Messaging.Redis;

public sealed class RedisOptions
{
    public const string SectionName = "Redis";

    public string ConnectionString { get; set; } = "localhost:6379,abortConnect=false";
    public string OutboxPendingQueueKey { get; set; } = "cashflow:transactions:outbox:pending";
    public string OutboxProcessingQueueKey { get; set; } = "cashflow:transactions:outbox:processing";
    public string OutboxDeadLetterQueueKey { get; set; } = "cashflow:transactions:outbox:dlq";
    public int PollDelayMilliseconds { get; set; } = 500;
    public int RetryDelayMilliseconds { get; set; } = 5000;
}

