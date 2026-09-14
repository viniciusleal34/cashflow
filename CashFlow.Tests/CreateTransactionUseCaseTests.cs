using CashFlow.Application.Transactions;
using CashFlow.Domain.Transactions;

namespace CashFlow.Tests;

public class CreateTransactionUseCaseTests
{
    [Fact]
    public async Task ExecuteAsync_ShouldPersistAndPublish_WhenRequestIsValid()
    {
        var repository = new InMemoryTransactionRepository();
        var unitOfWork = new FakeUnitOfWork();
        var publisher = new FakePublisher();
        var useCase = new CreateTransactionUseCase(repository, unitOfWork, publisher);

        var before = DateTime.UtcNow;
        var request = new CreateTransactionRequest(120m, "Credit", "salary");
        var result = await useCase.ExecuteAsync(request, CancellationToken.None);
        var after = DateTime.UtcNow;

        Assert.Equal("Credit", result.Type);
        Assert.Single(repository.Items);
        Assert.Equal(1, unitOfWork.SaveCalls);
        Assert.Equal(1, publisher.PublishCalls);
        Assert.Equal(repository.Items[0].OccurredAtUtc, result.OccurredAtUtc);
        Assert.Equal(repository.Items[0].OccurredAtUtc, publisher.LastEvent!.OccurredAtUtc);
        Assert.InRange(result.OccurredAtUtc, before.AddSeconds(-1), after.AddSeconds(1));
        Assert.Equal(DateTimeKind.Utc, result.OccurredAtUtc.Kind);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldThrow_WhenTypeIsInvalid()
    {
        var repository = new InMemoryTransactionRepository();
        var unitOfWork = new FakeUnitOfWork();
        var publisher = new FakePublisher();
        var useCase = new CreateTransactionUseCase(repository, unitOfWork, publisher);

        var request = new CreateTransactionRequest(120m, "Unknown", "invalid");

        await Assert.ThrowsAsync<ArgumentException>(() => useCase.ExecuteAsync(request, CancellationToken.None));
        Assert.Empty(repository.Items);
        Assert.Equal(0, unitOfWork.SaveCalls);
        Assert.Equal(0, publisher.PublishCalls);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldNotFail_WhenPublisherThrows()
    {
        var repository = new InMemoryTransactionRepository();
        var unitOfWork = new FakeUnitOfWork();
        var publisher = new FakePublisher { ThrowOnPublish = true };
        var useCase = new CreateTransactionUseCase(repository, unitOfWork, publisher);

        var request = new CreateTransactionRequest(50m, "Debit", "market");
        var result = await useCase.ExecuteAsync(request, CancellationToken.None);

        Assert.Equal("Debit", result.Type);
        Assert.Single(repository.Items);
        Assert.Equal(1, unitOfWork.SaveCalls);
        Assert.Equal(1, publisher.PublishCalls);
    }

    private sealed class InMemoryTransactionRepository : ITransactionRepository
    {
        public List<CashTransactions> Items { get; } = new();

        public Task AddAsync(CashTransactions transaction, CancellationToken cancellationToken)
        {
            Items.Add(transaction);
            return Task.CompletedTask;
        }
    }

    private sealed class FakeUnitOfWork : IUnitOfWork
    {
        public int SaveCalls { get; private set; }

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken)
        {
            SaveCalls++;
            return Task.FromResult(1);
        }
    }

    private sealed class FakePublisher : ITransactionEventPublisher
    {
        public int PublishCalls { get; private set; }
        public bool ThrowOnPublish { get; set; }
        public TransactionCreatedIntegrationEvent? LastEvent { get; private set; }

        public Task PublishAsync(TransactionCreatedIntegrationEvent integrationEvent, CancellationToken cancellationToken)
        {
            PublishCalls++;
            LastEvent = integrationEvent;

            if (ThrowOnPublish)
            {
                throw new InvalidOperationException("Kafka unavailable");
            }

            return Task.CompletedTask;
        }
    }
}

