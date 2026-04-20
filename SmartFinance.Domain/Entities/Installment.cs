using SmartFinance.Domain.ValueObjects;

namespace SmartFinance.Domain.Entities;

public class Installment : BaseEntity
{
    public Guid InstallmentPlanId { get; private set; }
    public int CurrentNumber { get; private set; }
    public Money Amount { get; private set; }
    public DateTime DueDate { get; private set; }
    public bool IsPaid { get; private set; }

    public virtual InstallmentPlan InstallmentPlan { get; private set; }

    protected Installment() { }

    public Installment(Guid installmentPlanId, int currentNumber, Money amount, DateTime dueDate)
    {
        InstallmentPlanId = installmentPlanId;
        CurrentNumber = currentNumber;
        Amount = amount;
        DueDate = dueDate;
        IsPaid = false;
    }

    public void MarkAsPaid()
    {
        IsPaid = true;
        UpdatedAt = DateTime.UtcNow;
    }
}
