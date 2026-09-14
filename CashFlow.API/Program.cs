using CashFlow.Application.Balances;
using CashFlow.Application.Transactions;
using CashFlow.Infrastructure;
using CashFlow.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

await EnsureDatabaseCreatedAsync(app.Services, app.Logger, app.Lifetime.ApplicationStopping);

app.MapPost("/transactions", async (
    CreateTransactionRequest request,
    ICreateTransactionUseCase useCase,
    CancellationToken cancellationToken) =>
{
    try
    {
        var result = await useCase.ExecuteAsync(request, cancellationToken);
        return Results.Created($"/transactions/{result.Id}", result);
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
})
.WithTags("Transactions")
.WithOpenApi();

app.MapGet("/balances/daily/{date}", async (
    string date,
    IGetDailyBalanceUseCase useCase,
    CancellationToken cancellationToken) =>
{
    if (!DateOnly.TryParse(date, out var parsedDate))
    {
        return Results.BadRequest(new { error = "Date must be in yyyy-MM-dd format." });
    }

    var result = await useCase.ExecuteAsync(parsedDate, cancellationToken);
    return Results.Ok(result);
})
.WithTags("Balances")
.WithOpenApi();

app.Run();

static async Task EnsureDatabaseCreatedAsync(IServiceProvider services, ILogger logger, CancellationToken cancellationToken)
{
    const int maxAttempts = 10;
    var delay = TimeSpan.FromSeconds(2);

    for (var attempt = 1; attempt <= maxAttempts; attempt++)
    {
        try
        {
            using var scope = services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<CashFlowDbContext>();
            await dbContext.Database.EnsureCreatedAsync(cancellationToken);
            return;
        }
        catch (Exception ex) when (attempt < maxAttempts)
        {
            logger.LogWarning(ex, "Database not ready yet. Retrying in {Delay} (attempt {Attempt}/{MaxAttempts}).", delay, attempt, maxAttempts);
            await Task.Delay(delay, cancellationToken);
        }
    }

    using (var scope = services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<CashFlowDbContext>();
        await dbContext.Database.EnsureCreatedAsync(cancellationToken);
    }
}

