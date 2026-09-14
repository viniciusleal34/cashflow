namespace CashFlow.Application.Transactions;

public sealed record CreateTransactionRequest(
	decimal Amount,
	string Type,
	string? Description);

