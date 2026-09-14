using CashFlow.Domain.Transactions;

namespace CashFlow.Application.Transactions;

public sealed class CreateTransactionUseCase : ICreateTransactionUseCase
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITransactionEventPublisher _publisher;

    public CreateTransactionUseCase(
        ITransactionRepository transactionRepository,
        IUnitOfWork unitOfWork,
        ITransactionEventPublisher publisher)
    {
        _transactionRepository = transactionRepository;
        _unitOfWork = unitOfWork;
        _publisher = publisher;
    }

    public async Task<CreateTransactionResult> ExecuteAsync(CreateTransactionRequest request, CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<TransactionType>(request.Type, true, out var type))
        {
            throw new ArgumentException("Transaction type must be Credit or Debit.", nameof(request.Type));
        }

        var occurredAtUtc = DateTime.UtcNow;

        var transaction = CashTransactions.Create(
            request.Amount,
            type,
            occurredAtUtc,
            request.Description);

        await _transactionRepository.AddAsync(transaction, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var integrationEvent = new TransactionCreatedIntegrationEvent(
            transaction.Id,
            transaction.Amount,
            transaction.Type.ToString(),
            occurredAtUtc,
            transaction.Description);

        try
        {
            await _publisher.PublishAsync(integrationEvent, cancellationToken);
        }
        catch
        {
          
        }

        return new CreateTransactionResult(
            transaction.Id,
            transaction.Amount,
            transaction.Type.ToString(),
            occurredAtUtc,
            transaction.Description);
    }
}

