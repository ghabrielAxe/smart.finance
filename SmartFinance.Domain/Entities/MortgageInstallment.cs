using SmartFinance.Domain.ValueObjects;

namespace SmartFinance.Domain.Entities;

public class MortgageInstallment : BaseEntity
{
    public Guid MortgageId { get; private set; }
    public int InstallmentNumber { get; private set; }
    public DateTime DueDate { get; private set; }

    public Money PrincipalAmortization { get; private set; } // O que abate da dívida
    public Money InterestAmount { get; private set; } // O lucro do banco
    public Money TotalAmount { get; private set; } // O que você paga
    public Money RemainingBalance { get; private set; } // Saldo devedor após esta parcela

    public bool IsPaid { get; private set; }
    public DateTime? PaymentDate { get; private set; }

    public virtual Mortgage Mortgage { get; private set; }

    protected MortgageInstallment() { }

    public MortgageInstallment(
        Guid mortgageId,
        int installmentNumber,
        DateTime dueDate,
        Money principalAmortization,
        Money interestAmount,
        Money remainingBalance
    )
    {
        MortgageId = mortgageId;
        InstallmentNumber = installmentNumber;
        DueDate = dueDate;
        PrincipalAmortization = principalAmortization;
        InterestAmount = interestAmount;
        TotalAmount = new Money(
            principalAmortization.Amount + interestAmount.Amount,
            principalAmortization.Currency
        );
        RemainingBalance = remainingBalance;
        IsPaid = false;
    }

    public void MarkAsPaid(DateTime paymentDate)
    {
        IsPaid = true;
        PaymentDate = paymentDate;
        UpdatedAt = DateTime.UtcNow;
    }
}
