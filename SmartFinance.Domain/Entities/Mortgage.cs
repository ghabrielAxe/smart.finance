using SmartFinance.Domain.ValueObjects;

namespace SmartFinance.Domain.Entities;

public class Mortgage : BaseEntity
{
    public Guid ContractId { get; private set; }
    public string BankName { get; private set; }
    public Money PrincipalAmount { get; private set; } // Valor financiado (Saldo devedor inicial)
    public Percentage AnnualInterestRate { get; private set; } // Ex: 9.5%
    public int TermMonths { get; private set; } // Ex: 360 meses (30 anos)
    public DateTime StartDate { get; private set; }

    private readonly List<MortgageInstallment> _installments = new();
    public IReadOnlyCollection<MortgageInstallment> Installments => _installments.AsReadOnly();

    public virtual RealEstateContract Contract { get; private set; }

    protected Mortgage() { }

    public Mortgage(
        Guid contractId,
        string bankName,
        Money principalAmount,
        Percentage annualInterestRate,
        int termMonths,
        DateTime startDate
    )
    {
        ContractId = contractId;
        BankName = bankName;
        PrincipalAmount = principalAmount;
        AnnualInterestRate = annualInterestRate;
        TermMonths = termMonths;
        StartDate = startDate;
    }

    public void LoadInstallments(IEnumerable<MortgageInstallment> installments)
    {
        if (_installments.Any())
            throw new InvalidOperationException("As parcelas já foram geradas.");
        _installments.AddRange(installments);
    }
}
