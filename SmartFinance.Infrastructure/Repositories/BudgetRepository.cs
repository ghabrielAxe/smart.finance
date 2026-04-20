using Microsoft.EntityFrameworkCore;
using SmartFinance.Domain.Entities;
using SmartFinance.Domain.Repositories;
using SmartFinance.Infrastructure.Data;

namespace SmartFinance.Infrastructure.Repositories;

public class BudgetRepository : IBudgetRepository
{
    private readonly SmartFinanceDbContext _context;

    public BudgetRepository(SmartFinanceDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Budget budget, CancellationToken cancellationToken = default)
    {
        await _context.Budgets.AddAsync(budget, cancellationToken);
    }

    public async Task<Budget?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context
            .Budgets.Include(b => b.Category)
            .FirstOrDefaultAsync(b => b.Id == id, cancellationToken);
    }

    public async Task<Budget?> GetByCategoryIdAsync(
        Guid categoryId,
        CancellationToken cancellationToken = default
    )
    {
        return await _context
            .Budgets.Include(b => b.Category)
            .FirstOrDefaultAsync(b => b.CategoryId == categoryId, cancellationToken);
    }

    public Task UpdateAsync(Budget budget, CancellationToken cancellationToken = default)
    {
        _context.Budgets.Update(budget);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Budget budget, CancellationToken cancellationToken = default)
    {
        _context.Budgets.Remove(budget);
        return Task.CompletedTask;
    }
}
