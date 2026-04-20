namespace SmartFinance.Domain.Entities;

public enum InsightType
{
    HealthScore,
    BudgetAlert,
    AnomalyDetection,
}

public enum InsightSeverity
{
    Info,
    Warning,
    Critical,
    Success,
}

public class Insight : BaseEntity
{
    public InsightType Type { get; private set; }
    public InsightSeverity Severity { get; private set; }
    public string Title { get; private set; }
    public string Message { get; private set; }

    public string? ActionableDataJson { get; private set; }

    public DateTime ReferenceDate { get; private set; }

    protected Insight() { }

    public Insight(
        InsightType type,
        InsightSeverity severity,
        string title,
        string message,
        DateTime referenceDate,
        string? actionableDataJson = null
    )
    {
        Type = type;
        Severity = severity;
        Title = title;
        Message = message;
        ReferenceDate = referenceDate.Date;
        ActionableDataJson = actionableDataJson;
    }
}
