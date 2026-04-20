using FluentValidation;
using MediatR;
using SmartFinance.Domain.Entities;
using SmartFinance.Domain.Repositories;

namespace SmartFinance.Application.Accounts.Commands;

public record CreateAccountCommand(string Name, AccountType Type) : IRequest<Guid>;

public class CreateAccountCommandValidator : AbstractValidator<CreateAccountCommand>
{
    public CreateAccountCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("O nome da conta é obrigatório.")
            .MaximumLength(100)
            .WithMessage("O nome da conta não pode exceder 100 caracteres.");

        RuleFor(x => x.Type).IsInEnum().WithMessage("Tipo de conta inválido.");
    }
}

public class CreateAccountCommandHandler : IRequestHandler<CreateAccountCommand, Guid>
{
    private readonly IAccountRepository _accountRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateAccountCommandHandler(IAccountRepository accountRepository, IUnitOfWork unitOfWork)
    {
        _accountRepository = accountRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(
        CreateAccountCommand request,
        CancellationToken cancellationToken
    )
    {
        var account = new Account(request.Name, request.Type);

        await _accountRepository.AddAsync(account, cancellationToken);

        await _unitOfWork.CommitAsync(cancellationToken);

        return account.Id;
    }
}
