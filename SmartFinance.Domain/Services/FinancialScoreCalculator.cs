using SmartFinance.Domain.ValueObjects;

namespace SmartFinance.Domain.Services;

public interface IFinancialScoreCalculator
{
    FinancialHealthScore Calculate(
        decimal liquidAssets,
        decimal essentialMonthlyExpenses,
        decimal monthlyIncome,
        decimal monthlyExpenses,
        decimal monthlyDebtPayments,
        decimal expenseStdDev,
        decimal averageExpenses,
        decimal netWorthGrowth,
        decimal previousNetWorth
    );
}

public class FinancialScoreCalculator : IFinancialScoreCalculator
{
    public FinancialHealthScore Calculate(
        decimal liquidAssets,
        decimal essentialMonthlyExpenses,
        decimal monthlyIncome,
        decimal monthlyExpenses,
        decimal monthlyDebtPayments,
        decimal expenseStdDev,
        decimal averageExpenses,
        decimal netWorthGrowth,
        decimal previousNetWorth
    )
    {
        // 1. Liquidez (25 pontos)
        // Fórmula: $liquidityMonths = \frac{liquidAssets}{essentialMonthlyExpenses}$
        var liquidityMonths =
            essentialMonthlyExpenses > 0 ? liquidAssets / essentialMonthlyExpenses : 0;
        var liquidityScore = liquidityMonths switch
        {
            >= 6 => 25m,
            >= 3 => 15m + ((liquidityMonths - 3m) / 3m) * 10m,
            >= 1 => 5m + ((liquidityMonths - 1m) / 2m) * 10m,
            _ => liquidityMonths * 5m,
        };

        // 2. Taxa de Poupança (20 pontos)
        // Fórmula: $savingsRate = \frac{income - expenses}{income}$
        var savingsRate = monthlyIncome > 0 ? (monthlyIncome - monthlyExpenses) / monthlyIncome : 0;
        var savingsScore = savingsRate switch
        {
            >= 0.20m => 20m,
            > 0 => (savingsRate / 0.20m) * 20m,
            _ => 0m,
        };

        // 3. Endividamento (20 pontos)
        // Fórmula: $debtRatio = \frac{debtPayments}{income}$
        var debtRatio = monthlyIncome > 0 ? monthlyDebtPayments / monthlyIncome : 0;
        var debtScore = debtRatio switch
        {
            <= 0.20m => 20m,
            <= 0.35m => 10m + ((0.35m - debtRatio) / 0.15m) * 10m,
            <= 0.50m => ((0.50m - debtRatio) / 0.15m) * 10m,
            _ => 0m,
        };

        // 4. Estabilidade de Gastos (15 pontos)
        // Fórmula (Coeficiente de Variação): $CV = \frac{\sigma}{\mu}$
        var cv = averageExpenses > 0 ? expenseStdDev / averageExpenses : 0;
        var stabilityScore = cv switch
        {
            <= 0.15m => 15m,
            <= 0.30m => 8m + ((0.30m - cv) / 0.15m) * 7m,
            _ => Math.Max(0m, 8m - ((cv - 0.30m) * 10m)), // Penalização extra para alta volatilidade
        };

        // 5. Tendência Patrimonial (20 pontos)
        // Fórmula: $growthRate = \frac{netWorthGrowth}{previousNetWorth}$
        var growthRate = previousNetWorth > 0 ? netWorthGrowth / previousNetWorth : 0;
        var trendScore = growthRate switch
        {
            >= 0.10m => 20m,
            >= 0.05m => 10m + ((growthRate - 0.05m) / 0.05m) * 10m,
            > 0 => 5m + (growthRate / 0.05m) * 5m,
            _ => 0m,
        };

        return new FinancialHealthScore
        {
            LiquidityScore = liquidityScore,
            SavingsScore = savingsScore,
            DebtScore = debtScore,
            StabilityScore = stabilityScore,
            TrendScore = trendScore,
        };
    }
}
