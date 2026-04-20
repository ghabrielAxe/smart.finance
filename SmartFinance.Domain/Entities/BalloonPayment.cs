using SmartFinance.Domain.ValueObjects;

namespace SmartFinance.Domain.Entities;

public class BalloonPayment : BaseEntity
{
    public Guid ContractId { get; private set; }
    public string Description { get; private set; } // Ex: "Balão Anual", "Balão das Chaves"
    public DateTime DueDate { get; private set; }
    public Money BaseAmount { get; private set; }
    public Money AdjustedAmount { get; private set; }
    public bool IsPaid { get; private set; }
    public DateTime? PaymentDate { get; private set; }

    public virtual RealEstateContract Contract { get; private set; }

    protected BalloonPayment() { }

    public BalloonPayment(Guid contractId, string description, DateTime dueDate, Money baseAmount)
    {
        ContractId = contractId;
        Description = description;
        DueDate = dueDate;
        BaseAmount = baseAmount;
        AdjustedAmount = baseAmount;
        IsPaid = false;
    }

    public void ApplyIndexAdjustment(decimal accumulatedIndexRate)
    {
        if (IsPaid)
            return;
        var newAmount = BaseAmount.Amount * (1 + accumulatedIndexRate);
        AdjustedAmount = new Money(Math.Round(newAmount, 2), BaseAmount.Currency);
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkAsPaid(DateTime paymentDate)
    {
        IsPaid = true;
        PaymentDate = paymentDate;
        UpdatedAt = DateTime.UtcNow;
    }
}
