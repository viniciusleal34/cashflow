using CashFlow.Domain.Transactions;

namespace CashFlow.Tests;

public class CashTransactionsTests
{
    [Fact]
    public void Create_ShouldThrow_WhenAmountIsInvalid()
    {
        var action = () => CashTransactions.Create(Guid.NewGuid(), 0m, TransactionType.Credit, DateTime.UtcNow, "test");

        var exception = Assert.Throws<ArgumentException>(action);
        Assert.Equal("amount", exception.ParamName);
    }

    [Fact]
    public void Create_ShouldTrimDescription_AndKeepPositiveAmount()
    {
        var transaction = CashTransactions.Create(Guid.NewGuid(), 10.5m, TransactionType.Debit, DateTime.UtcNow, "  coffee  ");

        Assert.Equal(10.5m, transaction.Amount);
        Assert.Equal("coffee", transaction.Description);
        Assert.Equal(TransactionType.Debit, transaction.Type);
    }
}
