namespace CashFlow.Domain.Transactions;

public sealed class CashTransactions
{
    private CashTransactions(Guid id, decimal amount, TransactionType type, DateTime occurredAtUtc, string? description)
    {
        Id = id;
        Amount = amount;
        Type = type;
        OccurredAtUtc = occurredAtUtc;
        Description = description;
    }
    public Guid Id { get; }
    public decimal Amount { get; }
    public TransactionType Type { get; }
    public DateTime OccurredAtUtc { get; }
    public string? Description { get; }

    public static CashTransactions Create(decimal amount, TransactionType type, DateTime date, string? description)
    {
        if (amount <= 0)
        {
            throw new ArgumentException("Amount must be greater than zero.", nameof(amount));
        }

        var occurredAtUtc = date.Kind == DateTimeKind.Utc ? date : date.ToUniversalTime();

        return new CashTransactions(Guid.NewGuid(), amount, type, occurredAtUtc, description?.Trim());
    }
}