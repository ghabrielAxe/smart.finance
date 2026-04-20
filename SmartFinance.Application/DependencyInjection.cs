using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using SmartFinance.Application.Behaviors;
using SmartFinance.Application.Ingestion.Engines;
using SmartFinance.Application.Ingestion.Pipeline;
using SmartFinance.Domain.Services;

namespace SmartFinance.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining(typeof(DependencyInjection));

        services.AddMediatR(config =>
        {
            config.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly);

            config.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });

        services.AddScoped<IFinancialScoreCalculator, FinancialScoreCalculator>();
        services.AddScoped<ICashflowProjectionService, CashflowProjectionService>();
        services.AddScoped<IMortgageCalculator, MortgageCalculator>();

        services.AddScoped<EmailExtractionEngine>();
        services.AddScoped<CategorizationEngine>();
        services.AddScoped<IIngestionPipeline, IngestionPipeline>();

        return services;
    }
}
