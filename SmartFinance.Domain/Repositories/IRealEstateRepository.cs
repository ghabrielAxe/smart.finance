using SmartFinance.Domain.Entities;

namespace SmartFinance.Domain.Repositories;

public interface IRealEstateRepository
{
    Task AddContractAsync(
        RealEstateContract contract,
        CancellationToken cancellationToken = default
    );
    Task<RealEstateContract?> GetContractByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default
    );
    Task UpdateContractAsync(
        RealEstateContract contract,
        CancellationToken cancellationToken = default
    );
}
