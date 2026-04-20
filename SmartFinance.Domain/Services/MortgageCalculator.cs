using SmartFinance.Domain.Entities;
using SmartFinance.Domain.ValueObjects;

namespace SmartFinance.Domain.Services;

public interface IMortgageCalculator
{
    IEnumerable<MortgageInstallment> GeneratePriceTable(
        Guid mortgageId,
        Money principal,
        Percentage annualInterestRate,
        int months,
        DateTime startDate
    );
}

public class MortgageCalculator : IMortgageCalculator
{
    public IEnumerable<MortgageInstallment> GeneratePriceTable(
        Guid mortgageId,
        Money principal,
        Percentage annualInterestRate,
        int months,
        DateTime startDate
    )
    {
        var currency = principal.Currency;
        var monthlyRate = annualInterestRate.Value / 12m;

        var factor = (decimal)Math.Pow((double)(1 + monthlyRate), months);
        var pmt = principal.Amount * (monthlyRate * factor) / (factor - 1);

        var currentBalance = principal.Amount;

        for (int i = 1; i <= months; i++)
        {
            var interest = currentBalance * monthlyRate;
            var amortization = pmt - interest;

            currentBalance -= amortization;
            if (currentBalance < 0.01m)
                currentBalance = 0; // Ajuste de arredondamento final

            yield return new MortgageInstallment(
                mortgageId: mortgageId,
                installmentNumber: i,
                dueDate: startDate.AddMonths(i),
                principalAmortization: new Money(Math.Round(amortization, 2), currency),
                interestAmount: new Money(Math.Round(interest, 2), currency),
                remainingBalance: new Money(Math.Round(currentBalance, 2), currency)
            );
        }
    }
}
