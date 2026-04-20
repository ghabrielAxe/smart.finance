using Microsoft.EntityFrameworkCore;
using SmartFinance.Domain.Entities;
using SmartFinance.Domain.Repositories;
using SmartFinance.Infrastructure.Data;

namespace SmartFinance.Infrastructure.Repositories;

public class TransactionRepository : ITransactionRepository
{
    private readonly SmartFinanceDbContext _context;

    public TransactionRepository(SmartFinanceDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(
        Transaction transaction,
        CancellationToken cancellationToken = default
    )
    {
        await _context.Transactions.AddAsync(transaction, cancellationToken);
    }

    public async Task<bool> ExistsByIdempotencyKeyAsync(
        string idempotencyKey,
        CancellationToken cancellationToken = default
    )
    {
        if (string.IsNullOrWhiteSpace(idempotencyKey))
            return false;

        return await _context.Transactions.AnyAsync(
            t => t.IdempotencyKey == idempotencyKey,
            cancellationToken
        );
    }

    public async Task<Transaction?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default
    )
    {
        return await _context
            .Transactions.Include(t => t.Entries)
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
    }

    public Task UpdateAsync(Transaction transaction, CancellationToken cancellationToken = default)
    {
        _context.Transactions.Update(transaction);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Transaction transaction, CancellationToken cancellationToken = default)
    {
        _context.Transactions.Remove(transaction);
        return Task.CompletedTask;
    }
}
