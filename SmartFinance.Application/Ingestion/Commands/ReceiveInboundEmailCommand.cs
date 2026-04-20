using System.Text.Json;
using MediatR;
using SmartFinance.Application.Ingestion.Pipeline;
using SmartFinance.Application.Interfaces;
using SmartFinance.Domain.Entities;
using SmartFinance.Domain.Repositories;

namespace SmartFinance.Application.Ingestion.Commands;

public record ReceiveInboundEmailCommand(string MessageId, string From, string Subject, string Body)
    : IRequest<Guid?>;

public sealed class ReceiveInboundEmailCommandHandler(
    IFinancialEventLogRepository eventLogRepository,
    IUnitOfWork unitOfWork,
    IEventChannel eventChannel
) : IRequestHandler<ReceiveInboundEmailCommand, Guid?>
{
    private static readonly string[] TrustedDomains =
    [
        "@nubank.com.br",
        "@itau.com.br",
        "@bancointer.com.br",
    ];

    public async Task<Guid?> Handle(
        ReceiveInboundEmailCommand request,
        CancellationToken cancellationToken
    )
    {
        if (
            !TrustedDomains.Any(domain =>
                request.From.EndsWith(domain, StringComparison.OrdinalIgnoreCase)
            )
        )
        {
            return null;
        }

        var rawPayload = JsonSerializer.Serialize(
            new
            {
                id = request.MessageId,
                from = request.From,
                subject = request.Subject,
                body = request.Body,
            }
        );

        var eventLog = new FinancialEventLog(
            "SendGrid_InboundParse",
            FinancialEventType.EmailTransaction,
            rawPayload
        );

        await eventLogRepository.AddAsync(eventLog, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);

        await eventChannel.PublishAsync(eventLog.Id);

        return eventLog.Id;
    }
}
