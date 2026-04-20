using SmartFinance.Domain.ValueObjects;

namespace SmartFinance.Domain.Entities;

public enum EntryType
{
    Debit,
    Credit,
}

public class LedgerEntry : BaseEntity
{
    public Guid TransactionId { get; private set; }
    public Guid AccountId { get; private set; }
    public Money Amount { get; private set; }
    public EntryType Type { get; private set; }

    public virtual Transaction Transaction { get; private set; }
    public virtual Account Account { get; private set; }

    protected LedgerEntry() { }

    internal LedgerEntry(Guid transactionId, Guid accountId, Money amount, EntryType type)
    {
        TransactionId = transactionId;
        AccountId = accountId;
        Amount = amount;
        Type = type;
    }
}
