using SmartFinance.Domain.Entities;

namespace SmartFinance.Domain.Repositories;

public interface IBudgetRepository
{
    Task AddAsync(Budget budget, CancellationToken cancellationToken = default);
    Task<Budget?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Budget?> GetByCategoryIdAsync(
        Guid categoryId,
        CancellationToken cancellationToken = default
    );
    Task UpdateAsync(Budget budget, CancellationToken cancellationToken = default);
    Task DeleteAsync(Budget budget, CancellationToken cancellationToken = default);
}
