using System.Data;
using Dapper;
using MediatR;
using SmartFinance.Application.Interfaces;
using SmartFinance.Domain.Services;

namespace SmartFinance.Application.Insights.Queries;

public record FinancialHealthScoreDto(
    int FinalScore,
    string Classification,
    decimal LiquidityScore,
    decimal SavingsScore,
    decimal DebtScore,
    decimal StabilityScore,
    decimal TrendScore
);

public record GetFinancialHealthScoreQuery() : IRequest<FinancialHealthScoreDto>;

public class GetFinancialHealthScoreQueryHandler
    : IRequestHandler<GetFinancialHealthScoreQuery, FinancialHealthScoreDto>
{
    private readonly ISqlConnectionFactory _sqlConnectionFactory;
    private readonly IFinancialScoreCalculator _scoreCalculator;
    private readonly ICurrentUserService _currentUserService;

    public GetFinancialHealthScoreQueryHandler(
        ISqlConnectionFactory sqlConnectionFactory,
        IFinancialScoreCalculator scoreCalculator,
        ICurrentUserService currentUserService
    )
    {
        _sqlConnectionFactory = sqlConnectionFactory;
        _scoreCalculator = scoreCalculator;
        _currentUserService = currentUserService;
    }

    public async Task<FinancialHealthScoreDto> Handle(
        GetFinancialHealthScoreQuery request,
        CancellationToken cancellationToken
    )
    {
        using var connection = _sqlConnectionFactory.CreateConnection();

        const string sql =
            @"
            -- 1. Ativos Líquidos (Soma de Checking, Savings e Investment)
            SELECT COALESCE(SUM(CASE WHEN le.""Type"" = 0 THEN le.""Amount"" ELSE -le.""Amount"" END), 0)
            FROM ""Accounts"" a
            JOIN ""LedgerEntries"" le ON a.""Id"" = le.""AccountId""
            WHERE a.""Type"" IN (0, 1, 3) AND a.""UserId"" = @UserId;

            -- 2. Receita e Despesa do Mês Atual (Baseado no Ledger)
            SELECT 
                COALESCE(SUM(CASE WHEN le.""Type"" = 0 AND a.""Type"" = 0 THEN le.""Amount"" ELSE 0 END), 0) AS Income,
                COALESCE(SUM(CASE WHEN le.""Type"" = 1 AND a.""Type"" = 0 THEN le.""Amount"" ELSE 0 END), 0) AS Expenses
            FROM ""LedgerEntries"" le
            JOIN ""Accounts"" a ON le.""AccountId"" = a.""Id""
            JOIN ""Transactions"" t ON le.""TransactionId"" = t.""Id""
            WHERE a.""UserId"" = @UserId
              AND EXTRACT(MONTH FROM t.""Date"") = EXTRACT(MONTH FROM CURRENT_DATE)
              AND EXTRACT(YEAR FROM t.""Date"") = EXTRACT(YEAR FROM CURRENT_DATE);

            -- 3. Histórico de 6 meses para Estabilidade (Volatilidade)
            SELECT COALESCE(SUM(le.""Amount""), 0)
            FROM ""Transactions"" t
            JOIN ""LedgerEntries"" le ON t.""Id"" = le.""TransactionId""
            JOIN ""Accounts"" a ON le.""AccountId"" = a.""Id""
            WHERE a.""UserId"" = @UserId 
              AND le.""Type"" = 1 AND a.""Type"" = 0
              AND t.""Date"" >= CURRENT_DATE - INTERVAL '6 months'
            GROUP BY EXTRACT(YEAR FROM t.""Date""), EXTRACT(MONTH FROM t.""Date"");

            -- 4. Pagamentos de Dívidas (Mortgage + Installments) no mês atual
            SELECT COALESCE(SUM(le.""Amount""), 0)
            FROM ""LedgerEntries"" le
            JOIN ""Transactions"" t ON le.""TransactionId"" = t.""Id""
            WHERE t.""UserId"" = @UserId 
              AND le.""Type"" = 1 
              AND (t.""InstallmentId"" IS NOT NULL OR t.""Description"" ILIKE '%Financiamento%')
              AND EXTRACT(MONTH FROM t.""Date"") = EXTRACT(MONTH FROM CURRENT_DATE);
        ";

        using var multi = await connection.QueryMultipleAsync(
            sql,
            new { UserId = _currentUserService.UserId }
        );

        var liquidAssets = await multi.ReadSingleAsync<decimal>();

        var currentMonth = await multi.ReadSingleAsync<dynamic>();
        decimal monthlyIncome = currentMonth.income;
        decimal monthlyExpenses = currentMonth.expenses;

        var history = (await multi.ReadAsync<decimal>()).ToList();
        var monthlyDebtPayments = await multi.ReadSingleAsync<decimal>();

        decimal avgExpenses = history.Any() ? history.Average() : monthlyExpenses;
        decimal stdDev = CalculateStandardDeviation(history, avgExpenses);

        decimal netWorthGrowth = monthlyIncome - monthlyExpenses;
        decimal previousNetWorth = liquidAssets - netWorthGrowth;

        var score = _scoreCalculator.Calculate(
            liquidAssets,
            essentialMonthlyExpenses: monthlyExpenses * 0.7m,
            monthlyIncome,
            monthlyExpenses,
            monthlyDebtPayments,
            stdDev,
            avgExpenses,
            netWorthGrowth,
            previousNetWorth
        );

        return new FinancialHealthScoreDto(
            score.FinalScore,
            score.Classification,
            score.LiquidityScore,
            score.SavingsScore,
            score.DebtScore,
            score.StabilityScore,
            score.TrendScore
        );
    }

    private decimal CalculateStandardDeviation(List<decimal> values, decimal avg)
    {
        if (values.Count < 2)
            return 0;
        var sum = values.Sum(v => (v - avg) * (v - avg));
        return (decimal)Math.Sqrt((double)(sum / values.Count));
    }
}
