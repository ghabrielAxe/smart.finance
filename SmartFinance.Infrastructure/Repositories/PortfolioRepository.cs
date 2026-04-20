using Microsoft.EntityFrameworkCore;
using SmartFinance.Domain.Entities;
using SmartFinance.Domain.Repositories;
using SmartFinance.Infrastructure.Data;

namespace SmartFinance.Infrastructure.Repositories;

public class PortfolioRepository : IPortfolioRepository
{
    private readonly SmartFinanceDbContext _context;

    public PortfolioRepository(SmartFinanceDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(
        PortfolioPosition position,
        CancellationToken cancellationToken = default
    )
    {
        await _context.PortfolioPositions.AddAsync(position, cancellationToken);
    }

    public async Task<PortfolioPosition?> GetByAccountAndAssetAsync(
        Guid accountId,
        Guid assetId,
        CancellationToken cancellationToken = default
    )
    {
        return await _context.PortfolioPositions.FirstOrDefaultAsync(
            p => p.AccountId == accountId && p.AssetId == assetId,
            cancellationToken
        );
    }
}
