using CashFlow.Application.Balances;
using CashFlow.Application.Transactions;
using CashFlow.Infrastructure.Messaging.Kafka;
using CashFlow.Infrastructure.Messaging.Redis;
using CashFlow.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace CashFlow.Infrastructure;

public static class InfrastructureServiceCollectionExtensions
{
	public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
	{
		services.AddDbContext<CashFlowDbContext>(options =>
			options.UseNpgsql(configuration.GetConnectionString("CashFlow")));

		services.Configure<KafkaOptions>(configuration.GetSection(KafkaOptions.SectionName));
		services.Configure<RedisOptions>(configuration.GetSection(RedisOptions.SectionName));

		services.AddSingleton<IConnectionMultiplexer>(_ =>
			ConnectionMultiplexer.Connect(configuration.GetSection(RedisOptions.SectionName).GetValue<string>(nameof(RedisOptions.ConnectionString)) ?? "localhost:6379,abortConnect=false"));

		services.AddScoped<ITransactionRepository, TransactionRepository>();
		services.AddScoped<IUnitOfWork, EfUnitOfWork>();
		services.AddScoped<IDailyBalanceRepository, DailyBalanceRepository>();

		services.AddScoped<ICreateTransactionUseCase, CreateTransactionUseCase>();
		services.AddScoped<IGetDailyBalanceUseCase, GetDailyBalanceUseCase>();

		services.AddSingleton<ITransactionEventPublisher, RedisTransactionOutboxPublisher>();
		services.AddSingleton<KafkaTransactionEventPublisher>();
		services.AddHostedService<RedisTransactionOutboxDispatcherHostedService>();
		services.AddHostedService<TransactionConsolidatorHostedService>();

		return services;
	}
}

