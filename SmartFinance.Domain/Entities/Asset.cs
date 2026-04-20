namespace SmartFinance.Domain.Entities;

public enum AssetClass
{
    Liquidity, // Caixinhas, Saldo Remunerado
    Stock, // Ações
    REIT, // FIIs
    FixedIncome, // Tesouro Direto, CDB, LCI, Debêntures
    PhysicalAsset, // Compras para revenda, Imóveis de aluguel, Relógios
}

public enum TradeType
{
    Buy,
    Sell,
    Dividend,
    Yield,
    Split,
}

public class Asset : BaseEntity
{
    public string TickerOrName { get; private set; }
    public AssetClass Class { get; private set; }
    public string Currency { get; private set; }
    public DateTime? MaturityDate { get; private set; }

    protected Asset() { }

    public Asset(
        string tickerOrName,
        AssetClass assetClass,
        string currency = "BRL",
        DateTime? maturityDate = null
    )
    {
        TickerOrName = tickerOrName;
        Class = assetClass;
        Currency = currency;
        MaturityDate = maturityDate;
    }
}
