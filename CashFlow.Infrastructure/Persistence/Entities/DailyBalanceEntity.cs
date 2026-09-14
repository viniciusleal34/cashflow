namespace CashFlow.Infrastructure.Persistence.Entities;

public sealed class DailyBalanceEntity
{
    public DateOnly Date { get; set; }
    public decimal Credits { get; set; }
    public decimal Debits { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
}

