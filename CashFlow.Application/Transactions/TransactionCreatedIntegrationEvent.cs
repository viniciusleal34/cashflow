namespace CashFlow.Application.Transactions;

public sealed record TransactionCreatedIntegrationEvent(
	Guid TransactionId,
	decimal Amount,
	string Type,
	DateTime OccurredAtUtc,
	string? Description);

