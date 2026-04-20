namespace SmartFinance.Domain.Entities;

public class AccountReconciliation : BaseEntity
{
    public Guid AccountId { get; private set; }
    public DateTime ClosingDate { get; private set; }
    public decimal ExpectedLedgerBalance { get; private set; }
    public decimal ActualBankBalance { get; private set; }
    public decimal Difference { get; private set; }

    public Guid? AdjustmentTransactionId { get; private set; }

    public virtual Account Account { get; private set; }

    protected AccountReconciliation() { }

    public AccountReconciliation(
        Guid accountId,
        DateTime closingDate,
        decimal expectedLedgerBalance,
        decimal actualBankBalance,
        Guid? adjustmentTransactionId = null
    )
    {
        AccountId = accountId;
        ClosingDate = closingDate.Date;
        ExpectedLedgerBalance = expectedLedgerBalance;
        ActualBankBalance = actualBankBalance;
        Difference = actualBankBalance - expectedLedgerBalance;
        AdjustmentTransactionId = adjustmentTransactionId;
    }
}
