namespace CashFlow.Application.Balances;

public sealed class GetDailyBalanceUseCase : IGetDailyBalanceUseCase
{
    private readonly IDailyBalanceRepository _dailyBalanceRepository;

    public GetDailyBalanceUseCase(IDailyBalanceRepository dailyBalanceRepository)
    {
        _dailyBalanceRepository = dailyBalanceRepository;
    }

    public async Task<GetDailyBalanceResult> ExecuteAsync(DateOnly date, CancellationToken cancellationToken)
    {
        var balance = await _dailyBalanceRepository.GetByDateAsync(date, cancellationToken);

        if (balance is null)
        {
            return new GetDailyBalanceResult(date, 0m, 0m, 0m);
        }

        return new GetDailyBalanceResult(balance.Date, balance.Credits, balance.Debits, balance.Total);
    }
}

