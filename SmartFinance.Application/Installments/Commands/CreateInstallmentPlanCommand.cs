using FluentValidation;
using MediatR;
using SmartFinance.Domain.Entities;
using SmartFinance.Domain.Repositories;
using SmartFinance.Domain.ValueObjects;

namespace SmartFinance.Application.Installments.Commands;

public record CreateInstallmentPlanCommand(
    string Description,
    decimal TotalAmount,
    string Currency,
    int TotalInstallments,
    DateTime FirstDueDate
) : IRequest<Guid>;

public class CreateInstallmentPlanCommandValidator : AbstractValidator<CreateInstallmentPlanCommand>
{
    public CreateInstallmentPlanCommandValidator()
    {
        RuleFor(x => x.Description).NotEmpty().MaximumLength(255);
        RuleFor(x => x.TotalAmount).GreaterThan(0);
        RuleFor(x => x.TotalInstallments).GreaterThan(1).LessThanOrEqualTo(72); // Máximo de 72 vezes
        RuleFor(x => x.Currency).NotEmpty().Length(3);
    }
}

public class CreateInstallmentPlanCommandHandler
    : IRequestHandler<CreateInstallmentPlanCommand, Guid>
{
    private readonly IInstallmentPlanRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateInstallmentPlanCommandHandler(
        IInstallmentPlanRepository repository,
        IUnitOfWork unitOfWork
    )
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(
        CreateInstallmentPlanCommand request,
        CancellationToken cancellationToken
    )
    {
        var totalMoney = new Money(request.TotalAmount, request.Currency);
        var plan = new InstallmentPlan(request.Description, totalMoney, request.TotalInstallments);
        
        var baseInstallmentAmount = Math.Round(request.TotalAmount / request.TotalInstallments, 2);

        var totalCalculated = baseInstallmentAmount * request.TotalInstallments;
        var difference = request.TotalAmount - totalCalculated;

        for (int i = 1; i <= request.TotalInstallments; i++)
        {
            var dueDate = request.FirstDueDate.AddMonths(i - 1);
            var currentAmount = baseInstallmentAmount;

            if (i == request.TotalInstallments)
            {
                currentAmount += difference;
            }

            var installment = new Installment(
                plan.Id,
                i,
                new Money(currentAmount, request.Currency),
                dueDate
            );

            plan.AddInstallment(installment);
        }

        await _repository.AddAsync(plan, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);

        return plan.Id;
    }
}
