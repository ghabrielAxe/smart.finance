using Microsoft.Extensions.DependencyInjection;
using SmartFinance.Workers.BackgroundServices;

namespace SmartFinance.Workers;

public static class DependencyInjection
{
    public static IServiceCollection AddWorkers(this IServiceCollection services)
    {
        services.AddHostedService<EventProcessingWorker>();

        return services;
    }
}
