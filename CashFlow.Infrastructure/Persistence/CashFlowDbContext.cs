using CashFlow.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace CashFlow.Infrastructure.Persistence;

public sealed class CashFlowDbContext : DbContext
{
    public CashFlowDbContext(DbContextOptions<CashFlowDbContext> options) : base(options)
    {
    }

    public DbSet<TransactionEntity> Transactions => Set<TransactionEntity>();
    public DbSet<DailyBalanceEntity> DailyBalances => Set<DailyBalanceEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TransactionEntity>(entity =>
        {
            entity.ToTable("transactions");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Amount).HasPrecision(18, 2);
            entity.Property(x => x.Type).HasMaxLength(20).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(300);
            entity.Property(x => x.OccurredAtUtc).IsRequired();
            entity.Property(x => x.CreatedAtUtc).IsRequired();
            entity.HasIndex(x => x.OccurredAtUtc);
        });

        modelBuilder.Entity<DailyBalanceEntity>(entity =>
        {
            entity.ToTable("daily_balances");
            entity.HasKey(x => x.Date);
            entity.Property(x => x.Credits).HasPrecision(18, 2);
            entity.Property(x => x.Debits).HasPrecision(18, 2);
            entity.Property(x => x.UpdatedAtUtc).IsRequired();
        });
    }
}

