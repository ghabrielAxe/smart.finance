using SmartFinance.Domain.Entities;

namespace SmartFinance.Domain.Repositories;

public interface IAccountRepository
{
    Task<Account> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(Account account, CancellationToken cancellationToken = default);
    Task UpdateAsync(Account account, CancellationToken cancellationToken = default);

    Task<Guid> GetSystemExpenseAccountIdAsync(CancellationToken cancellationToken = default);
    Task<Guid> GetSystemIncomeAccountIdAsync(CancellationToken cancellationToken = default);

    Task AddReconciliationAsync(
        AccountReconciliation reconciliation,
        CancellationToken cancellationToken = default
    );
}
