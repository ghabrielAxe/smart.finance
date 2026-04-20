using Microsoft.EntityFrameworkCore;
using SmartFinance.Domain.Entities;
using SmartFinance.Domain.Repositories;
using SmartFinance.Infrastructure.Data;

namespace SmartFinance.Infrastructure.Repositories;

public class BankAccountMappingRepository : IBankAccountMappingRepository
{
    private readonly SmartFinanceDbContext _context;

    public BankAccountMappingRepository(SmartFinanceDbContext context)
    {
        _context = context;
    }

    public async Task<BankAccountMapping?> GetByBankNameAsync(
        string bankName,
        CancellationToken cancellationToken = default
    )
    {
        return await _context.BankAccountMappings.FirstOrDefaultAsync(
            b => b.BankName.ToLower() == bankName.ToLower(),
            cancellationToken
        );
    }
}
