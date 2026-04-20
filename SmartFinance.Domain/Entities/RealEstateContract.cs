using SmartFinance.Domain.ValueObjects;

namespace SmartFinance.Domain.Entities;

public class RealEstateContract : BaseEntity
{
    public string PropertyName { get; private set; }
    public Money PropertyValue { get; private set; }
    public DateTime ContractDate { get; private set; }
    public DateTime ExpectedDeliveryDate { get; private set; }

    public virtual Mortgage Mortgage { get; private set; }

    private readonly List<ConstructionInstallment> _constructionInstallments = new();
    public IReadOnlyCollection<ConstructionInstallment> ConstructionInstallments =>
        _constructionInstallments.AsReadOnly();

    private readonly List<BalloonPayment> _balloonPayments = new();
    public IReadOnlyCollection<BalloonPayment> BalloonPayments => _balloonPayments.AsReadOnly();

    protected RealEstateContract() { }

    public RealEstateContract(
        string propertyName,
        Money propertyValue,
        DateTime contractDate,
        DateTime expectedDeliveryDate
    )
    {
        PropertyName = propertyName;
        PropertyValue = propertyValue;
        ContractDate = contractDate;
        ExpectedDeliveryDate = expectedDeliveryDate;
    }

    public void AddConstructionInstallment(ConstructionInstallment installment)
    {
        _constructionInstallments.Add(installment);
    }

    public void AddBalloonPayment(BalloonPayment balloonPayment)
    {
        _balloonPayments.Add(balloonPayment);
    }

    public void SetMortgage(Mortgage mortgage)
    {
        if (DateTime.UtcNow.Date < ExpectedDeliveryDate.Date && mortgage != null)
            throw new InvalidOperationException(
                "O financiamento bancário geralmente só é iniciado na entrega das chaves."
            );

        Mortgage = mortgage;
        UpdatedAt = DateTime.UtcNow;
    }
}
