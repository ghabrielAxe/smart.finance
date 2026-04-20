using SmartFinance.Domain.ValueObjects;

namespace SmartFinance.Domain.Entities;

public enum BudgetRolloverType
{
    None,
    CarryOver,
    ResetMonthly,
}

public class Budget : BaseEntity
{
    public Guid CategoryId { get; private set; }
    public Money MonthlyLimit { get; private set; }
    public Percentage WarningThreshold { get; private set; }
    public Percentage CriticalThreshold { get; private set; }
    public BudgetRolloverType RolloverType { get; private set; }

    public virtual Category Category { get; private set; }

    protected Budget() { }

    public Budget(
        Guid categoryId,
        Money monthlyLimit,
        Percentage warningThreshold,
        Percentage criticalThreshold,
        BudgetRolloverType rolloverType = BudgetRolloverType.None
    )
    {
        CategoryId = categoryId;
        MonthlyLimit = monthlyLimit;
        WarningThreshold = warningThreshold;
        CriticalThreshold = criticalThreshold;
        RolloverType = rolloverType;
    }

    public void Update(
        Money newLimit,
        Percentage newWarning,
        Percentage newCritical,
        BudgetRolloverType newRollover
    )
    {
        MonthlyLimit = newLimit;
        WarningThreshold = newWarning;
        CriticalThreshold = newCritical;
        RolloverType = newRollover;
        SetUpdatedAt();
    }
}
