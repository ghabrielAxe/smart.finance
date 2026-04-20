using SmartFinance.Domain.ValueObjects;

namespace SmartFinance.Domain.Entities;

public class Transaction : BaseEntity
{
    public DateTime Date { get; private set; }
    public string Description { get; private set; }

    public Guid? CategoryId { get; private set; }
    public Guid? InstallmentId { get; private set; }
    public string? IdempotencyKey { get; private set; }

    private readonly List<LedgerEntry> _entries = new();
    public IReadOnlyCollection<LedgerEntry> Entries => _entries.AsReadOnly();

    public virtual Category? Category { get; private set; }
    public virtual Installment? Installment { get; private set; }

    protected Transaction() { }

    public Transaction(
        DateTime date,
        string description,
        Guid? categoryId = null,
        Guid? installmentId = null,
        string? idempotencyKey = null
    )
    {
        Date = date;
        Description = description;
        CategoryId = categoryId;
        InstallmentId = installmentId;
        IdempotencyKey = idempotencyKey;
    }

    public void AddEntry(Guid accountId, Money amount, EntryType type)
    {
        _entries.Add(new LedgerEntry(Id, accountId, amount, type));
    }

    public bool ValidateAccountingEquation()
    {
        var debits = _entries.Where(e => e.Type == EntryType.Debit).Sum(e => e.Amount.Amount);
        var credits = _entries.Where(e => e.Type == EntryType.Credit).Sum(e => e.Amount.Amount);
        return debits == credits; // Garante o Princípio das Partidas Dobradas
    }

    public void UpdateBasicInfo(DateTime date, string description, Guid? categoryId)
    {
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("A descrição é obrigatória.");

        Date = date;
        Description = description;
        CategoryId = categoryId;
        SetUpdatedAt();
    }

    public static Transaction CreateExpense(
        DateTime date,
        string description,
        Guid sourceAccountId,
        Guid expenseAccountId,
        Money amount,
        Guid? categoryId = null,
        string? idempotencyKey = null
    )
    {
        var transaction = new Transaction(date, description, categoryId, null, idempotencyKey);

        // Sai da conta origem (Crédito)
        transaction.AddEntry(sourceAccountId, amount, EntryType.Credit);

        // Entra na conta de despesa consolidada (Débito)
        transaction.AddEntry(expenseAccountId, amount, EntryType.Debit);

        if (!transaction.ValidateAccountingEquation())
            throw new InvalidOperationException("Falha contábil ao instanciar despesa.");

        return transaction;
    }
}
