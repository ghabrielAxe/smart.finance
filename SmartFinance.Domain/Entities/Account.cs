namespace SmartFinance.Domain.Entities;

public enum AccountType
{
    Checking,
    Savings,
    CreditCard,
    Investment,
    Expense,
    Income,
}

public class Account : BaseEntity
{
    public string Name { get; private set; }
    public AccountType Type { get; private set; }
    public DateTime? LockedUntil { get; private set; }

    protected Account() { }

    public Account(string name, AccountType type)
    {
        Name = name;
        Type = type;
    }

    public void UpdateName(string newName)
    {
        if (string.IsNullOrWhiteSpace(newName))
            throw new ArgumentException("O nome da conta não pode ser vazio.");

        Name = newName;
    }

    public void LockPeriod(DateTime closingDate)
    {
        if (LockedUntil.HasValue && closingDate <= LockedUntil.Value)
            throw new InvalidOperationException(
                "A nova data de fechamento deve ser posterior ao último fechamento."
            );

        LockedUntil = closingDate.Date;
        SetUpdatedAt();
    }
}
