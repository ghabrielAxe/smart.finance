namespace SmartFinance.Domain.ValueObjects;

public record FinancialHealthScore
{
    public decimal LiquidityScore { get; init; }
    public decimal SavingsScore { get; init; }
    public decimal DebtScore { get; init; }
    public decimal StabilityScore { get; init; }
    public decimal TrendScore { get; init; }

    public int FinalScore
    {
        get
        {
            var total = LiquidityScore + SavingsScore + DebtScore + StabilityScore + TrendScore;
            return (int)Math.Clamp(Math.Round(total, MidpointRounding.AwayFromZero), 0, 100);
        }
    }

    public string Classification =>
        FinalScore switch
        {
            >= 90 => "Excelente",
            >= 75 => "Muito saudável",
            >= 60 => "Estável",
            >= 40 => "Atenção",
            >= 20 => "Risco",
            _ => "Crítico",
        };
}
