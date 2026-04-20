namespace SmartFinance.Domain.Entities;

public class Trade : BaseEntity
{
    public Guid PortfolioPositionId { get; private set; }
    public TradeType Type { get; private set; }
    public DateTime Date { get; private set; }

    public decimal Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }
    public decimal FeesAndTaxes { get; private set; } 

    public decimal? RealizedPnL { get; private set; }

    protected Trade() { }

    public Trade(
        Guid portfolioPositionId,
        TradeType type,
        DateTime date,
        decimal quantity,
        decimal unitPrice,
        decimal feesAndTaxes,
        decimal? realizedPnL = null
    )
    {
        PortfolioPositionId = portfolioPositionId;
        Type = type;
        Date = date;
        Quantity = quantity;
        UnitPrice = unitPrice;
        FeesAndTaxes = feesAndTaxes;
        RealizedPnL = realizedPnL;
    }
}
