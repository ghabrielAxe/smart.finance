using SmartFinance.Application.Ingestion.Models;

namespace SmartFinance.Application.Ingestion.Interfaces;

public interface IEmailExtractor
{
    bool CanHandle(string fromEmail, string subject, string body);

    ExtractedTransaction? Extract(
        string eventId,
        string subject,
        string body,
        string fromEmail,
        DateTime date
    );
}
