using MediatR;
using SmartFinance.Domain.Repositories;

namespace SmartFinance.Application.Budgets.Commands;

public record DeleteBudgetCommand(Guid Id) : IRequest<bool>;

public class DeleteBudgetCommandHandler : IRequestHandler<DeleteBudgetCommand, bool>
{
    private readonly IBudgetRepository _budgetRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteBudgetCommandHandler(IBudgetRepository budgetRepository, IUnitOfWork unitOfWork)
    {
        _budgetRepository = budgetRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(DeleteBudgetCommand request, CancellationToken cancellationToken)
    {
        var budget = await _budgetRepository.GetByIdAsync(request.Id, cancellationToken);
        if (budget == null)
            throw new KeyNotFoundException("Orçamento não encontrado.");

        await _budgetRepository.DeleteAsync(budget, cancellationToken);
        return await _unitOfWork.CommitAsync(cancellationToken);
    }
}
