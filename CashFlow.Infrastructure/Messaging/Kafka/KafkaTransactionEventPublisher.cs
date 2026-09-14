using System.Text.Json;
using CashFlow.Application.Transactions;
using Confluent.Kafka;
using Microsoft.Extensions.Options;

namespace CashFlow.Infrastructure.Messaging.Kafka;

public sealed class KafkaTransactionEventPublisher : ITransactionEventPublisher, IDisposable
{
    private readonly KafkaOptions _options;
    private readonly IProducer<string, string> _producer;

    public KafkaTransactionEventPublisher(IOptions<KafkaOptions> options)
    {
        _options = options.Value;

        var config = new ProducerConfig
        {
            BootstrapServers = _options.BootstrapServers,
            Acks = Acks.All,
            EnableIdempotence = true,
            MessageSendMaxRetries = 5
        };

        _producer = new ProducerBuilder<string, string>(config).Build();
    }

    public async Task PublishAsync(TransactionCreatedIntegrationEvent integrationEvent, CancellationToken cancellationToken)
    {
        var payload = JsonSerializer.Serialize(integrationEvent);

        await _producer.ProduceAsync(
            _options.TransactionCreatedTopic,
            new Message<string, string>
            {
                Key = integrationEvent.TransactionId.ToString(),
                Value = payload
            },
            cancellationToken);
    }

    public void Dispose()
    {
        _producer.Flush(TimeSpan.FromSeconds(5));
        _producer.Dispose();
    }
}

