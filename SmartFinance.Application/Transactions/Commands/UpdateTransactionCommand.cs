using FluentValidation;
using MediatR;
using SmartFinance.Domain.Repositories;

namespace SmartFinance.Application.Transactions.Commands;

public record UpdateTransactionCommand(Guid Id, DateTime Date, string Description, Guid? CategoryId)
    : IRequest<bool>;

public class UpdateTransactionCommandValidator : AbstractValidator<UpdateTransactionCommand>
{
    public UpdateTransactionCommandValidator()
    {
        RuleFor(x => x.Description).NotEmpty().MaximumLength(255);
        RuleFor(x => x.Date).NotEmpty();
    }
}

public class UpdateTransactionCommandHandler : IRequestHandler<UpdateTransactionCommand, bool>
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateTransactionCommandHandler(
        ITransactionRepository transactionRepository,
        IUnitOfWork unitOfWork
    )
    {
        _transactionRepository = transactionRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(
        UpdateTransactionCommand request,
        CancellationToken cancellationToken
    )
    {
        var transaction = await _transactionRepository.GetByIdAsync(request.Id, cancellationToken);

        if (transaction == null)
            throw new KeyNotFoundException("Transação não encontrada.");

        transaction.UpdateBasicInfo(request.Date, request.Description, request.CategoryId);

        await _transactionRepository.UpdateAsync(transaction, cancellationToken);
        return await _unitOfWork.CommitAsync(cancellationToken);
    }
}
