using MediatR;
using SmartFinance.Domain.Services;
using SmartFinance.Domain.ValueObjects;

namespace SmartFinance.Application.RealEstate.Queries;

public record MortgageInstallmentDto(
    int Number,
    DateTime DueDate,
    decimal Amortization,
    decimal Interest,
    decimal Total,
    decimal Balance
);

public record MortgageSimulationDto(
    decimal TotalFinanced,
    decimal TotalInterest,
    decimal TotalToPay,
    IEnumerable<MortgageInstallmentDto> Installments
);

public record SimulateMortgageQuery(
    decimal Principal,
    string Currency,
    decimal AnnualInterestRate,
    int Months,
    DateTime StartDate
) : IRequest<MortgageSimulationDto>;

public class SimulateMortgageQueryHandler
    : IRequestHandler<SimulateMortgageQuery, MortgageSimulationDto>
{
    private readonly IMortgageCalculator _calculator;

    public SimulateMortgageQueryHandler(IMortgageCalculator calculator)
    {
        _calculator = calculator;
    }

    public Task<MortgageSimulationDto> Handle(
        SimulateMortgageQuery request,
        CancellationToken cancellationToken
    )
    {
        var principal = new Money(request.Principal, request.Currency);
        var interestRate = new Percentage(request.AnnualInterestRate / 100m);

        var installments = _calculator
            .GeneratePriceTable(
                Guid.Empty,
                principal,
                interestRate,
                request.Months,
                request.StartDate
            )
            .ToList();

        var dtos = installments.Select(i => new MortgageInstallmentDto(
            i.InstallmentNumber,
            i.DueDate,
            i.PrincipalAmortization.Amount,
            i.InterestAmount.Amount,
            i.TotalAmount.Amount,
            i.RemainingBalance.Amount
        ));

        var totalInterest = installments.Sum(i => i.InterestAmount.Amount);
        var totalToPay = installments.Sum(i => i.TotalAmount.Amount);

        return Task.FromResult(
            new MortgageSimulationDto(request.Principal, totalInterest, totalToPay, dtos)
        );
    }
}
