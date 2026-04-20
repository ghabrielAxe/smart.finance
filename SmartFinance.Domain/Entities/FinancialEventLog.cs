namespace SmartFinance.Domain.Entities;

public enum FinancialEventType
{
    EmailTransaction,
    PixReceived,
    OpenFinanceSync,
}

public enum EventProcessingStatus
{
    Pending,
    Processed,
    Failed,
}

public class FinancialEventLog : BaseEntity
{
    public string Source { get; private set; } // Ex: "SendGrid_Email"
    public FinancialEventType Type { get; private set; }
    public string RawPayload { get; private set; }
    public EventProcessingStatus Status { get; private set; }
    public string? ErrorMessage { get; private set; }

    protected FinancialEventLog() { }

    public FinancialEventLog(string source, FinancialEventType type, string rawPayload)
    {
        Source = source;
        Type = type;
        RawPayload = rawPayload;
        Status = EventProcessingStatus.Pending;
    }

    public void MarkAsProcessed()
    {
        Status = EventProcessingStatus.Processed;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkAsFailed(string error)
    {
        Status = EventProcessingStatus.Failed;
        ErrorMessage = error;
        UpdatedAt = DateTime.UtcNow;
    }
}
