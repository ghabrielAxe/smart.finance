using SmartFinance.Domain.Entities;

namespace SmartFinance.Domain.Repositories;

public interface IBankAccountMappingRepository
{
    Task<BankAccountMapping?> GetByBankNameAsync(
        string bankName,
        CancellationToken cancellationToken = default
    );
}
