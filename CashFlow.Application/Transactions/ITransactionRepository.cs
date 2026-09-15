using CashFlow.Domain.Transactions;

namespace CashFlow.Application.Transactions;

public interface ITransactionRepository
{
	Task AddAsync(CashTransactions transaction, CancellationToken cancellationToken);
	Task<CashTransactions?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
}

