using SmartFinance.Domain.Entities;

namespace SmartFinance.Domain.Repositories;

public interface ITradeRepository
{
    Task AddAsync(Trade trade, CancellationToken cancellationToken = default);
}
