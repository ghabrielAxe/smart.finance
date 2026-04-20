using Dapper;
using FluentValidation;
using MediatR;
using SmartFinance.Application.Interfaces;
using SmartFinance.Domain.Entities;
using SmartFinance.Domain.Repositories;
using SmartFinance.Domain.ValueObjects;

namespace SmartFinance.Application.Accounts.Commands;

public record CloseAccountingPeriodCommand(
    Guid AccountId,
    int Month,
    int Year,
    decimal ActualBankBalance
) : IRequest<Guid>;

public class CloseAccountingPeriodCommandValidator : AbstractValidator<CloseAccountingPeriodCommand>
{
    public CloseAccountingPeriodCommandValidator()
    {
        RuleFor(x => x.AccountId).NotEmpty();
        RuleFor(x => x.Month).InclusiveBetween(1, 12);
        RuleFor(x => x.Year).InclusiveBetween(2000, 2100);
    }
}

public sealed class CloseAccountingPeriodCommandHandler(
    IAccountRepository accountRepository,
    ITransactionRepository transactionRepository,
    ISqlConnectionFactory sqlFactory,
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUserService
) : IRequestHandler<CloseAccountingPeriodCommand, Guid>
{
    public async Task<Guid> Handle(
        CloseAccountingPeriodCommand request,
        CancellationToken cancellationToken
    )
    {
        var account = await accountRepository.GetByIdAsync(request.AccountId, cancellationToken);

        var closingDate = new DateTime(
            request.Year,
            request.Month,
            DateTime.DaysInMonth(request.Year, request.Month)
        );

        if (account.LockedUntil.HasValue && closingDate <= account.LockedUntil.Value)
            throw new InvalidOperationException("Este período já se encontra fechado e auditado.");

        var expectedBalance = await CalculateLedgerBalanceUpToDateAsync(
            request.AccountId,
            closingDate
        );
        var difference = request.ActualBankBalance - expectedBalance;

        Guid? adjustmentTxId = null;

        if (difference != 0)
        {
            adjustmentTxId = await CreateAdjustmentTransactionAsync(
                request.AccountId,
                difference,
                closingDate,
                cancellationToken
            );
        }

        var reconciliation = new AccountReconciliation(
            request.AccountId,
            closingDate,
            expectedBalance,
            request.ActualBankBalance,
            adjustmentTxId
        );


        await accountRepository.AddReconciliationAsync(reconciliation, cancellationToken);

        account.LockPeriod(closingDate);
        await accountRepository.UpdateAsync(account, cancellationToken);

        await unitOfWork.CommitAsync(cancellationToken);

        return reconciliation.Id;
    }

    private async Task<decimal> CalculateLedgerBalanceUpToDateAsync(
        Guid accountId,
        DateTime closingDate
    )
    {
        using var connection = sqlFactory.CreateConnection();

        const string sql = """
            SELECT 
                COALESCE(SUM(CASE WHEN le."Type" = 0 THEN le."Amount" ELSE 0 END), 0) - 
                COALESCE(SUM(CASE WHEN le."Type" = 1 THEN le."Amount" ELSE 0 END), 0)
            FROM "LedgerEntries" le
            JOIN "Transactions" t ON le."TransactionId" = t."Id"
            WHERE le."AccountId" = @AccountId AND t."Date" <= @ClosingDate;
            """;

        return await connection.ExecuteScalarAsync<decimal>(
            sql,
            new { AccountId = accountId, ClosingDate = closingDate }
        );
    }

    private async Task<Guid> CreateAdjustmentTransactionAsync(
        Guid accountId,
        decimal difference,
        DateTime date,
        CancellationToken ct
    )
    {

        var isIncome = difference > 0;
        var absoluteDifference = Math.Abs(difference);
        var money = new Money(absoluteDifference, "BRL");

        var description = isIncome
            ? "Ajuste de Conciliação Automática (Rendimento)"
            : "Ajuste de Conciliação Automática (Despesa Oculta)";

        var systemContraAccountId = isIncome
            ? await accountRepository.GetSystemIncomeAccountIdAsync(ct)
            : await accountRepository.GetSystemExpenseAccountIdAsync(ct);

        var transaction = new Transaction(
            date,
            description,
            null,
            null,
            $"RECONCILIATION-{accountId}-{date:yyyyMMdd}"
        );

        if (isIncome)
        {
            // Entra na conta (Débito de Ativo) e sai da conta Sistêmica de Receita (Crédito)
            transaction.AddEntry(accountId, money, EntryType.Debit);
            transaction.AddEntry(systemContraAccountId, money, EntryType.Credit);
        }
        else
        {
            // Sai da conta (Crédito de Ativo) e entra na conta Sistêmica de Despesa (Débito)
            transaction.AddEntry(accountId, money, EntryType.Credit);
            transaction.AddEntry(systemContraAccountId, money, EntryType.Debit);
        }

        await transactionRepository.AddAsync(transaction, ct);
        return transaction.Id;
    }
}
