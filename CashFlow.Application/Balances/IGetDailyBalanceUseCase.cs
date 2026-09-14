namespace CashFlow.Application.Balances;

public interface IGetDailyBalanceUseCase
{
    Task<GetDailyBalanceResult> ExecuteAsync(DateOnly date, CancellationToken cancellationToken);
}

