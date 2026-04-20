using SmartFinance.Domain.ValueObjects;

namespace SmartFinance.Domain.Entities;

public class ConstructionInstallment : BaseEntity
{
    public Guid ContractId { get; private set; }
    public DateTime DueDate { get; private set; }
    public Money BaseAmount { get; private set; }
    public Money AdjustedAmount { get; private set; } // Valor com INCC aplicado
    public bool IsPaid { get; private set; }
    public DateTime? PaymentDate { get; private set; }

    public virtual RealEstateContract Contract { get; private set; }

    protected ConstructionInstallment() { }

    public ConstructionInstallment(Guid contractId, DateTime dueDate, Money baseAmount)
    {
        ContractId = contractId;
        DueDate = dueDate;
        BaseAmount = baseAmount;
        AdjustedAmount = baseAmount; // Inicialmente, o valor ajustado é igual ao base
        IsPaid = false;
    }

    public void ApplyIndexAdjustment(decimal accumulatedIndexRate)
    {
        if (IsPaid)
            throw new InvalidOperationException(
                "Não é possível reajustar uma parcela que já foi paga."
            );

        var newAmount = BaseAmount.Amount * (1 + accumulatedIndexRate);
        AdjustedAmount = new Money(Math.Round(newAmount, 2), BaseAmount.Currency);
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkAsPaid(DateTime paymentDate, Money actualPaidAmount)
    {
        IsPaid = true;
        PaymentDate = paymentDate;
        AdjustedAmount = actualPaidAmount; // Atualiza para o valor exato que saiu do bolso
        UpdatedAt = DateTime.UtcNow;
    }
}
