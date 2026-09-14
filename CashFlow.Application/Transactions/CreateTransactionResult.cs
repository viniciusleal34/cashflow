namespace CashFlow.Application.Transactions;

public sealed record CreateTransactionResult(
	Guid Id,
	decimal Amount,
	string Type,
	DateTime OccurredAtUtc,
	string? Description);

