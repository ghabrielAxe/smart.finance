namespace SmartFinance.Domain.Entities;

public class BankAccountMapping : BaseEntity
{
    public string BankName { get; private set; } // Ex: "Nubank"
    public string? CardLastDigits { get; private set; } // Ex: "1234"
    public Guid AccountId { get; private set; }

    public virtual Account Account { get; private set; }

    protected BankAccountMapping() { }

    public BankAccountMapping(string bankName, Guid accountId, string? cardLastDigits = null)
    {
        BankName = bankName;
        AccountId = accountId;
        CardLastDigits = cardLastDigits;
    }
}
