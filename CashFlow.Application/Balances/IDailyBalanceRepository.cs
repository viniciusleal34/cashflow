using CashFlow.Domain.Balances;

namespace CashFlow.Application.Balances;

public interface IDailyBalanceRepository
{
    Task<DailyBalance?> GetByDateAsync(DateOnly date, CancellationToken cancellationToken);
}

