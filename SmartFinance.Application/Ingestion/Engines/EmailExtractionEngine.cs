using SmartFinance.Application.Ingestion.Interfaces;
using SmartFinance.Application.Ingestion.Models;

namespace SmartFinance.Application.Ingestion.Engines;

public sealed class EmailExtractionEngine(IEnumerable<IEmailExtractor> extractors)
{
    public ExtractedTransaction? Extract(
        string eventId,
        string subject,
        string body,
        string fromEmail,
        DateTime date
    )
    {
        var extractor = extractors.FirstOrDefault(e => e.CanHandle(fromEmail, subject, body));
        return extractor?.Extract(eventId, subject, body, fromEmail, date);
    }
}
