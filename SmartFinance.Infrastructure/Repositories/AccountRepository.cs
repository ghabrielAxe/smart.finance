using Microsoft.EntityFrameworkCore;
using SmartFinance.Domain.Entities;
using SmartFinance.Domain.Repositories;
using SmartFinance.Infrastructure.Data;

namespace SmartFinance.Infrastructure.Repositories;

public sealed class AccountRepository(SmartFinanceDbContext context) : IAccountRepository
{
    public async Task<Account> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var account = await context.Accounts.FirstOrDefaultAsync(
            a => a.Id == id,
            cancellationToken
        );
        return account ?? throw new KeyNotFoundException($"Account with Id {id} not found.");
    }

    public async Task AddAsync(Account account, CancellationToken cancellationToken = default)
    {
        await context.Accounts.AddAsync(account, cancellationToken);
    }

    public Task UpdateAsync(Account account, CancellationToken cancellationToken = default)
    {
        context.Accounts.Update(account);
        return Task.CompletedTask;
    }

    public async Task<Guid> GetSystemExpenseAccountIdAsync(
        CancellationToken cancellationToken = default
    )
    {
        var expenseAccount = await context
            .Accounts.Where(a => a.Type == AccountType.Expense)
            .FirstOrDefaultAsync(cancellationToken);

        if (expenseAccount != null)
            return expenseAccount.Id;

        var newSystemExpenseAccount = new Account("Despesas Gerais", AccountType.Expense);
        await context.Accounts.AddAsync(newSystemExpenseAccount, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        return newSystemExpenseAccount.Id;
    }

    public async Task<Guid> GetSystemIncomeAccountIdAsync(
        CancellationToken cancellationToken = default
    )
    {
        var incomeAccount = await context
            .Accounts.Where(a => a.Type == AccountType.Income)
            .FirstOrDefaultAsync(cancellationToken);

        if (incomeAccount != null)
            return incomeAccount.Id;

        var newSystemIncomeAccount = new Account("Receitas Gerais", AccountType.Income);
        await context.Accounts.AddAsync(newSystemIncomeAccount, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        return newSystemIncomeAccount.Id;
    }

    public async Task AddReconciliationAsync(
        AccountReconciliation reconciliation,
        CancellationToken cancellationToken = default
    )
    {
        await context.AccountReconciliations.AddAsync(reconciliation, cancellationToken);
    }
}
