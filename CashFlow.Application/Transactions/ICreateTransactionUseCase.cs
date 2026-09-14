namespace CashFlow.Application.Transactions;

public interface ICreateTransactionUseCase
{
	Task<CreateTransactionResult> ExecuteAsync(CreateTransactionRequest request, CancellationToken cancellationToken);
}

