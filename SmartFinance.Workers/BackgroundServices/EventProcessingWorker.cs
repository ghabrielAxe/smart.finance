using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SmartFinance.Application.Ingestion.Pipeline;
using SmartFinance.Application.Interfaces;

namespace SmartFinance.Workers.BackgroundServices;

public sealed class EventProcessingWorker(
    IEventChannel channel,
    IServiceProvider serviceProvider,
    ILogger<EventProcessingWorker> logger
) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation(
            "EventProcessingWorker iniciado e aguardando eventos na fila em memória..."
        );

        await foreach (var eventId in channel.ReadAllAsync(stoppingToken))
        {
            try
            {
                logger.LogInformation("Iniciando processamento do Evento ID: {EventId}", eventId);

                using var scope = serviceProvider.CreateScope();
                var pipeline = scope.ServiceProvider.GetRequiredService<IIngestionPipeline>();

                await pipeline.ProcessEventAsync(eventId, stoppingToken);

                logger.LogInformation("Evento ID: {EventId} processado com sucesso.", eventId);
            }
            catch (Exception ex)
            {
                logger.LogError(
                    ex,
                    "Erro crítico isolado no processamento do evento {EventId}",
                    eventId
                );
            }
        }
    }
}
