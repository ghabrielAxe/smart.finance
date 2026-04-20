using Microsoft.EntityFrameworkCore;
using SmartFinance.Domain.Entities;
using SmartFinance.Domain.Repositories;
using SmartFinance.Infrastructure.Data;

namespace SmartFinance.Infrastructure.Repositories;

public class InstallmentPlanRepository : IInstallmentPlanRepository
{
    private readonly SmartFinanceDbContext _context;

    public InstallmentPlanRepository(SmartFinanceDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(InstallmentPlan plan, CancellationToken cancellationToken = default)
    {
        await _context.InstallmentPlans.AddAsync(plan, cancellationToken);
    }

    public async Task<InstallmentPlan?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default
    )
    {
        return await _context
            .InstallmentPlans.Include(p => p.Installments)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }
}
