using SmartFinance.Domain.ValueObjects;

namespace SmartFinance.Domain.Entities;

public class InstallmentPlan : BaseEntity
{
    public string Description { get; private set; }
    public Money TotalAmount { get; private set; }
    public int TotalInstallments { get; private set; }

    private readonly List<Installment> _installments = new();
    public IReadOnlyCollection<Installment> Installments => _installments.AsReadOnly();

    protected InstallmentPlan() { }

    public InstallmentPlan(string description, Money totalAmount, int totalInstallments)
    {
        Description = description;
        TotalAmount = totalAmount;
        TotalInstallments = totalInstallments;
    }

    public void AddInstallment(Installment installment)
    {
        _installments.Add(installment);
    }
}
