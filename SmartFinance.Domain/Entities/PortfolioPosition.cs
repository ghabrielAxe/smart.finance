namespace SmartFinance.Domain.Entities;

public class PortfolioPosition : BaseEntity
{
    public Guid AccountId { get; private set; } // Onde está? (Nubank, XP, BTG)
    public Guid AssetId { get; private set; } // O que é?

    public decimal Quantity { get; private set; } // Cotas, Unidades ou Saldo (no caso da caixinha)
    public decimal AverageCost { get; private set; } // PREÇO MÉDIO (Crítico para IR)

    public decimal TotalInvested => Quantity * AverageCost; // Book Value

    public virtual Asset Asset { get; private set; }

    protected PortfolioPosition() { }

    public PortfolioPosition(Guid accountId, Guid assetId)
    {
        AccountId = accountId;
        AssetId = assetId;
        Quantity = 0;
        AverageCost = 0;
    }

    // A lógica contábil de compra
    public void RecordBuy(decimal quantity, decimal pricePerUnit)
    {
        if (quantity <= 0 || pricePerUnit < 0)
            throw new ArgumentException("Quantidade e Preço devem ser positivos.");

        // Cálculo exato de Preço Médio Ponderado (Padrão Receita Federal Brasileira)
        var currentTotalCost = Quantity * AverageCost;
        var newAcquisitionCost = quantity * pricePerUnit;

        Quantity += quantity;
        AverageCost = (currentTotalCost + newAcquisitionCost) / Quantity;

        SetUpdatedAt();
    }

    // A lógica contábil de venda que RETORNA o impacto fiscal
    public RealizedGainResult RecordSell(decimal quantitySold, decimal salePricePerUnit)
    {
        if (quantitySold <= 0 || quantitySold > Quantity)
            throw new InvalidOperationException(
                "Quantidade de venda inválida ou maior que a custódia atual."
            );

        var totalCostOfSoldAssets = quantitySold * AverageCost;
        var totalSaleGross = quantitySold * salePricePerUnit;

        // PnL (Profit and Loss) Realizado
        var profitOrLoss = totalSaleGross - totalCostOfSoldAssets;

        // O Preço Médio NÃO se altera numa venda. Apenas a quantidade baixa.
        Quantity -= quantitySold;

        SetUpdatedAt();

        return new RealizedGainResult(profitOrLoss, totalSaleGross, totalCostOfSoldAssets);
    }
}

public record RealizedGainResult(decimal PnL, decimal GrossValue, decimal CostBasis);
