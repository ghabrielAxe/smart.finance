namespace SmartFinance.Domain.Entities;

public class RealEstateGoal : BaseEntity
{
    public string Title { get; private set; }
    public decimal TotalValue { get; private set; }
    public decimal DownPaymentTarget { get; private set; }
    public DateTime FinancingStartDate { get; private set; }

    protected RealEstateGoal() { }

    public RealEstateGoal(
        string title,
        decimal totalValue,
        decimal downPaymentTarget,
        DateTime financingStartDate
    )
    {
        Title = title;
        TotalValue = totalValue;
        DownPaymentTarget = downPaymentTarget;
        FinancingStartDate = financingStartDate;
    }
}
