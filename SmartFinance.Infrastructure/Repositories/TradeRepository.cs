using SmartFinance.Domain.Entities;
using SmartFinance.Domain.Repositories;
using SmartFinance.Infrastructure.Data;

namespace SmartFinance.Infrastructure.Repositories;

public class TradeRepository : ITradeRepository
{
    private readonly SmartFinanceDbContext _context;

    public TradeRepository(SmartFinanceDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Trade trade, CancellationToken cancellationToken = default)
    {
        await _context.Trades.AddAsync(trade, cancellationToken);
    }
}
