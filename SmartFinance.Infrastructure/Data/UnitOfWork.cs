using SmartFinance.Domain.Repositories;

namespace SmartFinance.Infrastructure.Data;

public class UnitOfWork : IUnitOfWork
{
    private readonly SmartFinanceDbContext _context;

    public UnitOfWork(SmartFinanceDbContext context)
    {
        _context = context;
    }

    public async Task<bool> CommitAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken) > 0;
    }
}
