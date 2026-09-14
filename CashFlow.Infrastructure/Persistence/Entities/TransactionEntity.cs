namespace CashFlow.Infrastructure.Persistence.Entities;

public sealed class TransactionEntity
{
    public Guid Id { get; set; }
    public decimal Amount { get; set; }
    public string Type { get; set; } = string.Empty;
    public DateTime OccurredAtUtc { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}

