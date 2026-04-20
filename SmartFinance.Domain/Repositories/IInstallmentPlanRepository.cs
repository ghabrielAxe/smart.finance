using SmartFinance.Domain.Entities;

namespace SmartFinance.Domain.Repositories;

public interface IInstallmentPlanRepository
{
    Task AddAsync(InstallmentPlan plan, CancellationToken cancellationToken = default);
    Task<InstallmentPlan?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}
