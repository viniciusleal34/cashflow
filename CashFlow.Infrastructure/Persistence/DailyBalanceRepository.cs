using CashFlow.Application.Balances;
using CashFlow.Domain.Balances;
using Microsoft.EntityFrameworkCore;

namespace CashFlow.Infrastructure.Persistence;

public sealed class DailyBalanceRepository : IDailyBalanceRepository
{
    private readonly CashFlowDbContext _dbContext;

    public DailyBalanceRepository(CashFlowDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<DailyBalance?> GetByDateAsync(DateOnly date, CancellationToken cancellationToken)
    {
        var entity = await _dbContext.DailyBalances
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Date == date, cancellationToken);

        return entity is null ? null : new DailyBalance(entity.Date, entity.Credits, entity.Debits);
    }
}

