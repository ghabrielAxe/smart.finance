using Microsoft.EntityFrameworkCore;
using SmartFinance.Domain.Entities;
using SmartFinance.Domain.Repositories;
using SmartFinance.Infrastructure.Data;

namespace SmartFinance.Infrastructure.Repositories;

public sealed class InsightRepository(SmartFinanceDbContext context) : IInsightRepository
{
    public async Task<bool> HasInsightForDateAsync(
        Guid userId,
        InsightType type,
        DateTime referenceDate,
        CancellationToken cancellationToken = default
    )
    {
        return await context.Insights.AnyAsync(
            i => i.UserId == userId && i.Type == type && i.ReferenceDate == referenceDate.Date,
            cancellationToken
        );
    }

    public async Task AddRangeAsync(
        IEnumerable<Insight> insights,
        CancellationToken cancellationToken = default
    )
    {
        await context.Insights.AddRangeAsync(insights, cancellationToken);
    }
}
