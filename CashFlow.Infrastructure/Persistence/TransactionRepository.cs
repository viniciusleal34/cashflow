using CashFlow.Application.Transactions;
using CashFlow.Domain.Transactions;
using CashFlow.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace CashFlow.Infrastructure.Persistence;

public sealed class TransactionRepository : ITransactionRepository
{
    private readonly CashFlowDbContext _dbContext;

    public TransactionRepository(CashFlowDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task AddAsync(CashTransactions transaction, CancellationToken cancellationToken)
    {
        var entity = new TransactionEntity
        {
            Id = transaction.Id,
            Amount = transaction.Amount,
            Type = transaction.Type.ToString(),
            OccurredAtUtc = transaction.OccurredAtUtc,
            Description = transaction.Description,
            CreatedAtUtc = DateTime.UtcNow
        };

        return _dbContext.Transactions.AddAsync(entity, cancellationToken).AsTask();
    }

    public async Task<CashTransactions?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var entity = await _dbContext.Transactions.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        return entity is null
            ? null
            : CashTransactions.Rehydrate(
                entity.Id,
                entity.Amount,
                Enum.Parse<TransactionType>(entity.Type, true),
                entity.OccurredAtUtc,
                entity.Description);
    }
}

