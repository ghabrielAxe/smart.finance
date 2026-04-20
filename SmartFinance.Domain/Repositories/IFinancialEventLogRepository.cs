using SmartFinance.Domain.Entities;

namespace SmartFinance.Domain.Repositories;

public interface IFinancialEventLogRepository
{
    Task<FinancialEventLog?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(FinancialEventLog eventLog, CancellationToken cancellationToken = default);
}
