namespace CashFlow.Application.Transactions;

public interface ITransactionEventPublisher
{
	Task PublishAsync(TransactionCreatedIntegrationEvent integrationEvent, CancellationToken cancellationToken);
}

