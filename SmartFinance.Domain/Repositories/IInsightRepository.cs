using SmartFinance.Domain.Entities;

namespace SmartFinance.Domain.Repositories;

public interface IInsightRepository
{
    Task<bool> HasInsightForDateAsync(
        Guid userId,
        InsightType type,
        DateTime referenceDate,
        CancellationToken cancellationToken = default
    );
    Task AddRangeAsync(
        IEnumerable<Insight> insights,
        CancellationToken cancellationToken = default
    );
}
