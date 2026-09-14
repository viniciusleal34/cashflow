namespace CashFlow.Application.Balances;

public sealed record GetDailyBalanceResult(
    DateOnly Date,
    decimal Credits,
    decimal Debits,
    decimal Total);

