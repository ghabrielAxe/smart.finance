using Microsoft.Extensions.DependencyInjection;
using SmartFinance.Application.Ingestion.Extractors;
using SmartFinance.Application.Ingestion.Interfaces;
using SmartFinance.Application.Interfaces;
using SmartFinance.Domain.Repositories;
using SmartFinance.Infrastructure.Data;
using SmartFinance.Infrastructure.Messaging;
using SmartFinance.Infrastructure.Repositories;

namespace SmartFinance.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton<ISqlConnectionFactory, SqlConnectionFactory>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddScoped<IAccountRepository, AccountRepository>();
        services.AddScoped<ITransactionRepository, TransactionRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();

        services.AddScoped<IFinancialEventLogRepository, FinancialEventLogRepository>();
        services.AddScoped<ICategoryRuleRepository, CategoryRuleRepository>();
        services.AddScoped<IBankAccountMappingRepository, BankAccountMappingRepository>();
        services.AddScoped<IRealEstateRepository, RealEstateRepository>();
        services.AddScoped<IInstallmentPlanRepository, InstallmentPlanRepository>();
        services.AddScoped<IBudgetRepository, BudgetRepository>();
        services.AddScoped<IInsightRepository, InsightRepository>();

        services.AddScoped<IEmailExtractor, NubankEmailExtractor>();

        services.AddSingleton<InMemoryEventChannel>();

        return services;
    }
}
