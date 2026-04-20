using SmartFinance.Domain.ValueObjects;

namespace SmartFinance.Domain.Entities;

public class RealEstateFinancing : BaseEntity
{
    public string Title { get; private set; }
    public Money TotalPropertyPrice { get; private set; }
    public DateTime FinancingStartDate { get; private set; } 

    private readonly List<RealEstateInstallment> _installments = new();
    public IReadOnlyCollection<RealEstateInstallment> Installments => _installments.AsReadOnly();

    protected RealEstateFinancing() { }

    public RealEstateFinancing(string title, Money totalPropertyPrice, DateTime financingStartDate)
    {
        Title = title;
        TotalPropertyPrice = totalPropertyPrice;
        FinancingStartDate = financingStartDate;
    }

    public void AddInstallment(InstallmentType type, Money amount, DateTime dueDate)
    {
        _installments.Add(new RealEstateInstallment(type, amount, dueDate));
    }


    public decimal GetTotalPaid() =>
        _installments.Where(i => i.IsPaid).Sum(i => i.AdjustedAmount.Amount);

    public decimal GetRemainingBalanceToKeys() =>
        _installments.Where(i => !i.IsPaid).Sum(i => i.AdjustedAmount.Amount);

    public Percentage GetDownPaymentProgress()
    {
        var totalDownPayment = _installments.Sum(i => i.AdjustedAmount.Amount);
        if (totalDownPayment == 0)
            return new Percentage(0);

        return new Percentage(GetTotalPaid() / totalDownPayment);
    }
}
