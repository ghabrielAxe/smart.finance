using FluentValidation;
using MediatR;
using SmartFinance.Domain.Entities;
using SmartFinance.Domain.Repositories;
using SmartFinance.Domain.ValueObjects;

namespace SmartFinance.Application.Budgets.Commands;

public record UpdateBudgetCommand(
    Guid Id,
    decimal MonthlyLimit,
    string Currency,
    decimal WarningThresholdPercentage,
    decimal CriticalThresholdPercentage,
    BudgetRolloverType RolloverType
) : IRequest<bool>;

public class UpdateBudgetCommandValidator : AbstractValidator<UpdateBudgetCommand>
{
    public UpdateBudgetCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.MonthlyLimit).GreaterThan(0);
        RuleFor(x => x.Currency).NotEmpty().Length(3);

        RuleFor(x => x.WarningThresholdPercentage).GreaterThan(0).LessThanOrEqualTo(1);
        RuleFor(x => x.CriticalThresholdPercentage)
            .GreaterThan(0)
            .LessThanOrEqualTo(1)
            .GreaterThan(x => x.WarningThresholdPercentage);

        RuleFor(x => x.RolloverType).IsInEnum();
    }
}

public class UpdateBudgetCommandHandler : IRequestHandler<UpdateBudgetCommand, bool>
{
    private readonly IBudgetRepository _budgetRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateBudgetCommandHandler(IBudgetRepository budgetRepository, IUnitOfWork unitOfWork)
    {
        _budgetRepository = budgetRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(UpdateBudgetCommand request, CancellationToken cancellationToken)
    {
        var budget = await _budgetRepository.GetByIdAsync(request.Id, cancellationToken);
        if (budget == null)
            throw new KeyNotFoundException("Orçamento não encontrado.");

        var newLimit = new Money(request.MonthlyLimit, request.Currency);
        var newWarning = new Percentage(request.WarningThresholdPercentage);
        var newCritical = new Percentage(request.CriticalThresholdPercentage);

        budget.Update(newLimit, newWarning, newCritical, request.RolloverType);

        await _budgetRepository.UpdateAsync(budget, cancellationToken);
        return await _unitOfWork.CommitAsync(cancellationToken);
    }
}
