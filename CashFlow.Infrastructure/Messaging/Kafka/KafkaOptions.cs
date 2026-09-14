namespace CashFlow.Infrastructure.Messaging.Kafka;

public sealed class KafkaOptions
{
    public const string SectionName = "Kafka";

    public string BootstrapServers { get; set; } = "localhost:9092";
    public string TransactionCreatedTopic { get; set; } = "cashflow.transactions.created";
    public string TransactionCreatedDlqTopic { get; set; } = "cashflow.transactions.created.dlq";
    public string ConsumerGroupId { get; set; } = "cashflow-balance-consolidator";
}

