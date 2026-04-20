using Microsoft.EntityFrameworkCore;
using SmartFinance.Domain.Entities;
using SmartFinance.Domain.Repositories;
using SmartFinance.Infrastructure.Data;

namespace SmartFinance.Infrastructure.Repositories;

public sealed class CategoryRuleRepository(SmartFinanceDbContext context) : ICategoryRuleRepository
{
    public async Task<IEnumerable<CategoryRule>> GetAllAsync(
        CancellationToken cancellationToken = default
    )
    {
        return await context
            .CategoryRules.Include(c => c.Category)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<CategoryRule?> GetByKeywordAsync(
        string keyword,
        CancellationToken cancellationToken = default
    )
    {
        var normalizedKeyword = keyword.ToLowerInvariant();

        return await context.CategoryRules.FirstOrDefaultAsync(
            c => c.Keyword == normalizedKeyword,
            cancellationToken
        );
    }

    public async Task AddAsync(CategoryRule rule, CancellationToken cancellationToken = default)
    {
        await context.CategoryRules.AddAsync(rule, cancellationToken);
    }

    public Task UpdateAsync(CategoryRule rule, CancellationToken cancellationToken = default)
    {
        context.CategoryRules.Update(rule);
        return Task.CompletedTask;
    }
}
