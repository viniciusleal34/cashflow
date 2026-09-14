namespace CashFlow.Domain.Balances;

public class DailyBalance
{
    public DailyBalance(DateOnly date, decimal credits, decimal debits)
    {
        Date = date;
        Credits = credits;
        Debits = debits;
    }

    public DateOnly Date { get; }
    public decimal Credits { get; }
    public decimal Debits { get; }
    public decimal Total => Credits - Debits;
}