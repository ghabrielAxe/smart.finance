using FluentValidation;
using MediatR;
using SmartFinance.Domain.Entities;
using SmartFinance.Domain.Repositories;
using SmartFinance.Domain.Services;
using SmartFinance.Domain.ValueObjects;

namespace SmartFinance.Application.RealEstate.Commands;

public record StartMortgageCommand(
    Guid ContractId,
    string BankName,
    decimal PrincipalAmount,
    string Currency,
    decimal AnnualInterestRate,
    int TermMonths,
    DateTime StartDate
) : IRequest<Guid>;

public class StartMortgageCommandValidator : AbstractValidator<StartMortgageCommand>
{
    public StartMortgageCommandValidator()
    {
        RuleFor(x => x.BankName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.PrincipalAmount).GreaterThan(0);
        RuleFor(x => x.AnnualInterestRate).GreaterThan(0).LessThan(100);
        RuleFor(x => x.TermMonths).GreaterThan(0).LessThanOrEqualTo(420); // Máx 35 anos
        RuleFor(x => x.Currency).NotEmpty().Length(3);
    }
}

public class StartMortgageCommandHandler : IRequestHandler<StartMortgageCommand, Guid>
{
    private readonly IRealEstateRepository _repository;
    private readonly IMortgageCalculator _calculator;
    private readonly IUnitOfWork _unitOfWork;

    public StartMortgageCommandHandler(
        IRealEstateRepository repository,
        IMortgageCalculator calculator,
        IUnitOfWork unitOfWork
    )
    {
        _repository = repository;
        _calculator = calculator;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(
        StartMortgageCommand request,
        CancellationToken cancellationToken
    )
    {
        var contract = await _repository.GetContractByIdAsync(
            request.ContractId,
            cancellationToken
        );
        if (contract == null)
            throw new KeyNotFoundException("Contrato imobiliário não encontrado.");

        var principal = new Money(request.PrincipalAmount, request.Currency);
        var interestRate = new Percentage(request.AnnualInterestRate / 100m);

        var mortgage = new Mortgage(
            contract.Id,
            request.BankName,
            principal,
            interestRate,
            request.TermMonths,
            request.StartDate
        );

        var installments = _calculator.GeneratePriceTable(
            mortgage.Id,
            principal,
            interestRate,
            request.TermMonths,
            request.StartDate
        );

        mortgage.LoadInstallments(installments);

        contract.SetMortgage(mortgage);

        await _repository.UpdateContractAsync(contract, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);

        return mortgage.Id;
    }
}
