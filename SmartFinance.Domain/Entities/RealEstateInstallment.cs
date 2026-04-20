using SmartFinance.Domain.ValueObjects;

namespace SmartFinance.Domain.Entities;

public enum InstallmentType
{
    Regular,
    Balloon,
    KeyDelivery,
}

public class RealEstateInstallment : BaseEntity
{
    public Guid RealEstateFinancingId { get; private set; }
    public InstallmentType Type { get; private set; }
    public Money OriginalAmount { get; private set; }
    public Money AdjustedAmount { get; private set; } // Valor com INCC aplicado
    public DateTime DueDate { get; private set; }
    public bool IsPaid { get; private set; }
    public DateTime? PaidDate { get; private set; }

    public virtual RealEstateFinancing Financing { get; private set; }

    protected RealEstateInstallment() { }

    internal RealEstateInstallment(InstallmentType type, Money amount, DateTime dueDate)
    {
        Type = type;
        OriginalAmount = amount;
        AdjustedAmount = amount;
        DueDate = dueDate;
        IsPaid = false;
    }

    public void ApplyAdjustment(Percentage adjustmentRate)
    {
        if (IsPaid)
            throw new InvalidOperationException("Não é possível reajustar uma parcela já paga.");

        var newAmount = OriginalAmount.Amount * (1 + adjustmentRate.Value);
        AdjustedAmount = new Money(newAmount, OriginalAmount.Currency);
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkAsPaid(DateTime paymentDate)
    {
        if (IsPaid)
            throw new InvalidOperationException("Parcela já está paga.");

        IsPaid = true;
        PaidDate = paymentDate.Date;
        UpdatedAt = DateTime.UtcNow;
    }
}
