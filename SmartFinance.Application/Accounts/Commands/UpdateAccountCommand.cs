using MediatR;
using SmartFinance.Domain.Repositories;

namespace SmartFinance.Application.Accounts.Commands;

public record UpdateAccountCommand(Guid Id, string Name) : IRequest<bool>;

public class UpdateAccountCommandHandler : IRequestHandler<UpdateAccountCommand, bool>
{
    private readonly IAccountRepository _accountRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateAccountCommandHandler(IAccountRepository accountRepository, IUnitOfWork unitOfWork)
    {
        _accountRepository = accountRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(
        UpdateAccountCommand request,
        CancellationToken cancellationToken
    )
    {
        var account = await _accountRepository.GetByIdAsync(request.Id, cancellationToken);

        // O Global Query Filter garante que ele não encontre contas de outros usuários
        account.UpdateName(request.Name);

        await _accountRepository.UpdateAsync(account, cancellationToken);
        return await _unitOfWork.CommitAsync(cancellationToken);
    }
}
