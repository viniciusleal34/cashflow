using CashFlow.Application.Balances;
using CashFlow.Application.Transactions;
using CashFlow.Infrastructure.Messaging.Kafka;
using CashFlow.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CashFlow.Infrastructure;

public static class InfrastructureServiceCollectionExtensions
{
	public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
	{
		services.AddDbContext<CashFlowDbContext>(options =>
			options.UseNpgsql(configuration.GetConnectionString("CashFlow")));

		services.Configure<KafkaOptions>(configuration.GetSection(KafkaOptions.SectionName));

		services.AddScoped<ITransactionRepository, TransactionRepository>();
		services.AddScoped<IUnitOfWork, EfUnitOfWork>();
		services.AddScoped<IDailyBalanceRepository, DailyBalanceRepository>();

		services.AddScoped<ICreateTransactionUseCase, CreateTransactionUseCase>();
		services.AddScoped<IGetDailyBalanceUseCase, GetDailyBalanceUseCase>();

		services.AddSingleton<ITransactionEventPublisher, KafkaTransactionEventPublisher>();
		services.AddHostedService<TransactionConsolidatorHostedService>();

		return services;
	}
}

