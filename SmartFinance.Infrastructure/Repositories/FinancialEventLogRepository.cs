using Microsoft.EntityFrameworkCore;
using SmartFinance.Domain.Entities;
using SmartFinance.Domain.Repositories;
using SmartFinance.Infrastructure.Data;

namespace SmartFinance.Infrastructure.Repositories;

public sealed class FinancialEventLogRepository(SmartFinanceDbContext context)
    : IFinancialEventLogRepository
{
    public async Task<FinancialEventLog?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default
    )
    {
        return await context.FinancialEventLogs.FirstOrDefaultAsync(
            e => e.Id == id,
            cancellationToken
        );
    }

    public async Task AddAsync(
        FinancialEventLog eventLog,
        CancellationToken cancellationToken = default
    )
    {
        await context.FinancialEventLogs.AddAsync(eventLog, cancellationToken);
    }
}
