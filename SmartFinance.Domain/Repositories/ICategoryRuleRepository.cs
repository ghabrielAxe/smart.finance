using SmartFinance.Domain.Entities;

namespace SmartFinance.Domain.Repositories;

public interface ICategoryRuleRepository
{
    Task<IEnumerable<CategoryRule>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<CategoryRule?> GetByKeywordAsync(
        string keyword,
        CancellationToken cancellationToken = default
    );
    Task AddAsync(CategoryRule rule, CancellationToken cancellationToken = default);
    Task UpdateAsync(CategoryRule rule, CancellationToken cancellationToken = default);
}
