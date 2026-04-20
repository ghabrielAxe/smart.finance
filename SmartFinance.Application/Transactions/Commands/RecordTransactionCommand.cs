using FluentValidation;
using MediatR;
using SmartFinance.Domain.Entities;
using SmartFinance.Domain.Repositories;
using SmartFinance.Domain.ValueObjects;

namespace SmartFinance.Application.Transactions.Commands;

public record LedgerEntryDto(Guid AccountId, decimal Amount, string Currency, EntryType Type);

public record RecordTransactionCommand(
    DateTime Date,
    string Description,
    Guid? CategoryId,
    Guid? InstallmentId,
    List<LedgerEntryDto> Entries
) : IRequest<Guid>;

public class RecordTransactionCommandValidator : AbstractValidator<RecordTransactionCommand>
{
    public RecordTransactionCommandValidator()
    {
        RuleFor(x => x.Description).NotEmpty().MaximumLength(255);
        RuleFor(x => x.Date).NotEmpty();
        RuleFor(x => x.Entries)
            .NotEmpty()
            .WithMessage(
                "A transação precisa ter pelo menos duas pernas no Ledger (Débito e Crédito)."
            )
            .Must(e => e.Count >= 2)
            .WithMessage("O padrão de Partidas Dobradas exige pelo menos 2 entradas.");

        RuleForEach(x => x.Entries)
            .ChildRules(entry =>
            {
                entry.RuleFor(e => e.AccountId).NotEmpty();
                entry
                    .RuleFor(e => e.Amount)
                    .GreaterThan(0)
                    .WithMessage("O valor da entrada deve ser maior que zero.");
                entry.RuleFor(e => e.Currency).NotEmpty().Length(3);
            });
    }
}

public class RecordTransactionCommandHandler : IRequestHandler<RecordTransactionCommand, Guid>
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RecordTransactionCommandHandler(
        ITransactionRepository transactionRepository,
        IUnitOfWork unitOfWork
    )
    {
        _transactionRepository = transactionRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(
        RecordTransactionCommand request,
        CancellationToken cancellationToken
    )
    {
        var transaction = new Transaction(
            request.Date,
            request.Description,
            request.CategoryId,
            request.InstallmentId
        );

        foreach (var entryDto in request.Entries)
        {
            var money = new Money(entryDto.Amount, entryDto.Currency);
            transaction.AddEntry(entryDto.AccountId, money, entryDto.Type);
        }

        if (!transaction.ValidateAccountingEquation())
            throw new InvalidOperationException(
                "A equação contábil deve fechar (Débitos = Créditos). Transação recusada."
            );

        await _transactionRepository.AddAsync(transaction, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);

        return transaction.Id;
    }
}
