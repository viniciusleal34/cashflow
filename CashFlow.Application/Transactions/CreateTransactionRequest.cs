namespace CashFlow.Application.Transactions;

public sealed record CreateTransactionRequest(
	Guid Id,
	decimal Amount,
	string Type,
	string? Description);

