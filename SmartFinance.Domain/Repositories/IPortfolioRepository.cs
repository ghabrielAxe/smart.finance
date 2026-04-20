using SmartFinance.Domain.Entities;

namespace SmartFinance.Domain.Repositories;

public interface IPortfolioRepository
{
    Task AddAsync(PortfolioPosition position, CancellationToken cancellationToken = default);
    Task<PortfolioPosition?> GetByAccountAndAssetAsync(
        Guid accountId,
        Guid assetId,
        CancellationToken cancellationToken = default
    );
}
