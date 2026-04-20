using MediatR;
using SmartFinance.Domain.Repositories;

namespace SmartFinance.Application.Transactions.Commands;

public record DeleteTransactionCommand(Guid Id) : IRequest<bool>;

public class DeleteTransactionCommandHandler : IRequestHandler<DeleteTransactionCommand, bool>
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteTransactionCommandHandler(
        ITransactionRepository transactionRepository,
        IUnitOfWork unitOfWork
    )
    {
        _transactionRepository = transactionRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(
        DeleteTransactionCommand request,
        CancellationToken cancellationToken
    )
    {
        var transaction = await _transactionRepository.GetByIdAsync(request.Id, cancellationToken);

        if (transaction == null)
            throw new KeyNotFoundException("Transação não encontrada.");

        await _transactionRepository.DeleteAsync(transaction, cancellationToken);
        return await _unitOfWork.CommitAsync(cancellationToken);
    }
}
