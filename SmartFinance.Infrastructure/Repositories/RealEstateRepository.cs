using Microsoft.EntityFrameworkCore;
using SmartFinance.Domain.Entities;
using SmartFinance.Domain.Repositories;
using SmartFinance.Infrastructure.Data;

namespace SmartFinance.Infrastructure.Repositories;

public class RealEstateRepository : IRealEstateRepository
{
    private readonly SmartFinanceDbContext _context;

    public RealEstateRepository(SmartFinanceDbContext context)
    {
        _context = context;
    }

    public async Task AddContractAsync(
        RealEstateContract contract,
        CancellationToken cancellationToken = default
    )
    {
        await _context.RealEstateContracts.AddAsync(contract, cancellationToken);
    }

    public async Task<RealEstateContract?> GetContractByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default
    )
    {
        return await _context
            .RealEstateContracts.Include(c => c.ConstructionInstallments)
            .Include(c => c.BalloonPayments)
            .Include(c => c.Mortgage)
                .ThenInclude(m => m.Installments)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public Task UpdateContractAsync(
        RealEstateContract contract,
        CancellationToken cancellationToken = default
    )
    {
        _context.RealEstateContracts.Update(contract);
        return Task.CompletedTask;
    }
}
