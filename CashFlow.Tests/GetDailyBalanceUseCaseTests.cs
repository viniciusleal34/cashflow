using CashFlow.Application.Balances;
using CashFlow.Domain.Balances;

namespace CashFlow.Tests;

public class GetDailyBalanceUseCaseTests
{
    [Fact]
    public async Task ExecuteAsync_ShouldReturnZeros_WhenNoBalanceExists()
    {
        var repository = new FakeDailyBalanceRepository();
        var useCase = new GetDailyBalanceUseCase(repository);

        var date = new DateOnly(2026, 9, 14);
        var result = await useCase.ExecuteAsync(date, CancellationToken.None);

        Assert.Equal(0m, result.Total);
        Assert.Equal(date, result.Date);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnConsolidatedValues_WhenBalanceExists()
    {
        var repository = new FakeDailyBalanceRepository
        {
            Value = new DailyBalance(new DateOnly(2026, 9, 14), 150m, 40m)
        };

        var useCase = new GetDailyBalanceUseCase(repository);
        var result = await useCase.ExecuteAsync(new DateOnly(2026, 9, 14), CancellationToken.None);

        Assert.Equal(150m, result.Credits);
        Assert.Equal(40m, result.Debits);
        Assert.Equal(110m, result.Total);
    }

    private sealed class FakeDailyBalanceRepository : IDailyBalanceRepository
    {
        public DailyBalance? Value { get; set; }

        public Task<DailyBalance?> GetByDateAsync(DateOnly date, CancellationToken cancellationToken)
        {
            return Task.FromResult(Value);
        }
    }
}

