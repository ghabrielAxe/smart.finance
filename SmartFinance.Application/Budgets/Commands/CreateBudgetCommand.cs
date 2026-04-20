using FluentValidation;
using MediatR;
using SmartFinance.Domain.Entities;
using SmartFinance.Domain.Repositories;
using SmartFinance.Domain.ValueObjects;

namespace SmartFinance.Application.Budgets.Commands;

public record CreateBudgetCommand(
    Guid CategoryId,
    decimal MonthlyLimit,
    string Currency,
    decimal WarningThresholdPercentage,
    decimal CriticalThresholdPercentage,
    BudgetRolloverType RolloverType
) : IRequest<Guid>;

public class CreateBudgetCommandValidator : AbstractValidator<CreateBudgetCommand>
{
    public CreateBudgetCommandValidator()
    {
        RuleFor(x => x.CategoryId).NotEmpty();
        RuleFor(x => x.MonthlyLimit).GreaterThan(0);
        RuleFor(x => x.Currency).NotEmpty().Length(3);

        RuleFor(x => x.WarningThresholdPercentage)
            .GreaterThan(0)
            .LessThanOrEqualTo(1)
            .WithMessage("O limite de aviso deve estar entre 0.01 e 1.00 (Ex: 0.8 para 80%).");

        RuleFor(x => x.CriticalThresholdPercentage)
            .GreaterThan(0)
            .LessThanOrEqualTo(1)
            .GreaterThan(x => x.WarningThresholdPercentage)
            .WithMessage("O limite crítico deve ser maior que o limite de aviso.");

        RuleFor(x => x.RolloverType).IsInEnum();
    }
}

public class CreateBudgetCommandHandler : IRequestHandler<CreateBudgetCommand, Guid>
{
    private readonly IBudgetRepository _budgetRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateBudgetCommandHandler(
        IBudgetRepository budgetRepository,
        ICategoryRepository categoryRepository,
        IUnitOfWork unitOfWork
    )
    {
        _budgetRepository = budgetRepository;
        _categoryRepository = categoryRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(CreateBudgetCommand request, CancellationToken cancellationToken)
    {
        var category = await _categoryRepository.GetByIdAsync(
            request.CategoryId,
            cancellationToken
        );
        if (category == null)
            throw new KeyNotFoundException("Categoria não encontrada.");

        var existingBudget = await _budgetRepository.GetByCategoryIdAsync(
            request.CategoryId,
            cancellationToken
        );
        if (existingBudget != null)
            throw new InvalidOperationException(
                "Já existe um orçamento definido para esta categoria."
            );

        var monthlyLimit = new Money(request.MonthlyLimit, request.Currency);
        var warning = new Percentage(request.WarningThresholdPercentage);
        var critical = new Percentage(request.CriticalThresholdPercentage);

        var budget = new Budget(
            request.CategoryId,
            monthlyLimit,
            warning,
            critical,
            request.RolloverType
        );

        await _budgetRepository.AddAsync(budget, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);

        return budget.Id;
    }
}
