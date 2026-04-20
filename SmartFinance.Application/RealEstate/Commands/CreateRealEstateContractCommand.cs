using FluentValidation;
using MediatR;
using SmartFinance.Domain.Entities;
using SmartFinance.Domain.Repositories;
using SmartFinance.Domain.ValueObjects;

namespace SmartFinance.Application.RealEstate.Commands;

// DTOs auxiliares para o Command
public record ConstructionInstallmentDto(DateTime DueDate, decimal Amount);

public record BalloonPaymentDto(string Description, DateTime DueDate, decimal Amount);

public record CreateRealEstateContractCommand(
    string PropertyName,
    decimal PropertyValue,
    string Currency,
    DateTime ContractDate,
    DateTime ExpectedDeliveryDate,
    List<ConstructionInstallmentDto> MonthlyInstallments,
    List<BalloonPaymentDto> BalloonPayments
) : IRequest<Guid>;

public class CreateRealEstateContractCommandValidator
    : AbstractValidator<CreateRealEstateContractCommand>
{
    public CreateRealEstateContractCommandValidator()
    {
        RuleFor(x => x.PropertyName).NotEmpty().MaximumLength(150);
        RuleFor(x => x.PropertyValue).GreaterThan(0);
        RuleFor(x => x.Currency).NotEmpty().Length(3);
        RuleFor(x => x.ExpectedDeliveryDate)
            .GreaterThan(x => x.ContractDate)
            .WithMessage("A data de entrega deve ser posterior à data do contrato.");
    }
}

public class CreateRealEstateContractCommandHandler
    : IRequestHandler<CreateRealEstateContractCommand, Guid>
{
    private readonly IRealEstateRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateRealEstateContractCommandHandler(
        IRealEstateRepository repository,
        IUnitOfWork unitOfWork
    )
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(
        CreateRealEstateContractCommand request,
        CancellationToken cancellationToken
    )
    {
        var propertyValue = new Money(request.PropertyValue, request.Currency);

        var contract = new RealEstateContract(
            request.PropertyName,
            propertyValue,
            request.ContractDate,
            request.ExpectedDeliveryDate
        );

        foreach (var inst in request.MonthlyInstallments)
        {
            var amount = new Money(inst.Amount, request.Currency);
            contract.AddConstructionInstallment(
                new ConstructionInstallment(contract.Id, inst.DueDate, amount)
            );
        }

        foreach (var balloon in request.BalloonPayments)
        {
            var amount = new Money(balloon.Amount, request.Currency);
            contract.AddBalloonPayment(
                new BalloonPayment(contract.Id, balloon.Description, balloon.DueDate, amount)
            );
        }

        await _repository.AddContractAsync(contract, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);

        return contract.Id;
    }
}
