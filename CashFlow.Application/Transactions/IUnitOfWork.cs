namespace CashFlow.Application.Transactions;

public interface IUnitOfWork
{
	Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}

