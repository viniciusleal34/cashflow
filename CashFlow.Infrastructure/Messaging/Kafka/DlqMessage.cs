namespace CashFlow.Infrastructure.Messaging.Kafka;

public sealed record DlqMessage(
    string OriginalMessage,
    string Error,
    DateTime FailedAtUtc);

