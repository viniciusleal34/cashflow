namespace CashFlow.Application.Transactions;

public sealed record CreateTransactionRequest(
	decimal Amount,
	string Type,
	DateTime OccurredAtUtc,
	string? Description);

