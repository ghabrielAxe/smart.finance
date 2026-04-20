using Dapper;
using MediatR;
using SmartFinance.Application.Interfaces;
using SmartFinance.Domain.Entities;
using SmartFinance.Domain.Repositories;

namespace SmartFinance.Application.Intelligence.Commands;

public record RunDailyIntelligenceCommand(Guid UserId, DateTime ExecutionDate) : IRequest<bool>;

file record MonthlyFlowDto(decimal TotalIncome, decimal TotalExpenses);

public sealed class RunDailyIntelligenceCommandHandler(
    ISqlConnectionFactory sqlFactory,
    IInsightRepository insightRepository,
    IUnitOfWork unitOfWork
) : IRequestHandler<RunDailyIntelligenceCommand, bool>
{
    public async Task<bool> Handle(
        RunDailyIntelligenceCommand request,
        CancellationToken cancellationToken
    )
    {
        var alreadyRan = await insightRepository.HasInsightForDateAsync(
            request.UserId,
            InsightType.HealthScore,
            request.ExecutionDate,
            cancellationToken
        );

        if (alreadyRan)
            return true;

        using var connection = sqlFactory.CreateConnection();

        const string metricsSql = """
            -- A. Liquidez Diária
            SELECT COALESCE(SUM(p."Quantity" * p."AverageCost"), 0)
            FROM "PortfolioPositions" p
            JOIN "Assets" a ON p."AssetId" = a."Id"
            WHERE p."UserId" = @UserId AND a."Class" = 0;

            -- B. Fluxo do Mês
            SELECT 
                COALESCE(SUM(CASE WHEN le."Type" = 0 THEN le."Amount" ELSE 0 END), 0) AS TotalIncome,
                COALESCE(SUM(CASE WHEN le."Type" = 1 THEN le."Amount" ELSE 0 END), 0) AS TotalExpenses
            FROM "Transactions" t
            JOIN "LedgerEntries" le ON t."Id" = le."TransactionId"
            WHERE t."UserId" = @UserId 
              AND t."Date" >= DATE_TRUNC('month', @ExecutionDate)
              AND t."Date" < @ExecutionDate + INTERVAL '1 day';

            -- C. Dívida Pendente
            SELECT COALESCE(SUM(i."Amount_Amount"), 0)
            FROM "InstallmentPlans" p
            JOIN "Installments" i ON p."Id" = i."InstallmentPlanId"
            WHERE p."UserId" = @UserId AND i."IsPaid" = false;
            """;

        using var multi = await connection.QueryMultipleAsync(
            metricsSql,
            new { request.UserId, request.ExecutionDate }
        );

        var liquidAssets = await multi.ReadSingleAsync<decimal>();
        var currentMonthFlow =
            await multi.ReadSingleOrDefaultAsync<MonthlyFlowDto>() ?? new MonthlyFlowDto(0, 0);
        var pendingDebt = await multi.ReadSingleAsync<decimal>();

        var score = CalculateHealthScore(
            liquidAssets,
            currentMonthFlow.TotalIncome,
            currentMonthFlow.TotalExpenses,
            pendingDebt
        );

        List<Insight> insightsToSave =
        [
            new Insight(
                InsightType.HealthScore,
                score >= 80 ? InsightSeverity.Success
                    : score >= 50 ? InsightSeverity.Warning
                    : InsightSeverity.Critical,
                "Fechamento Diário de Score",
                $"Seu score financeiro de hoje é {score:F0}/100.",
                request.ExecutionDate,
                $"{{\"Score\": {score}, \"Liquid\": {liquidAssets}, \"Debt\": {pendingDebt}}}"
            ),
        ];

        if (
            currentMonthFlow.TotalExpenses > currentMonthFlow.TotalIncome
            && currentMonthFlow.TotalIncome > 0
        )
        {
            insightsToSave.Add(
                new Insight(
                    InsightType.AnomalyDetection,
                    InsightSeverity.Warning,
                    "Atenção ao Fluxo de Caixa",
                    $"Você já gastou {currentMonthFlow.TotalExpenses:C} neste mês, o que ultrapassa suas entradas de {currentMonthFlow.TotalIncome:C}.",
                    request.ExecutionDate
                )
            );
        }

        await insightRepository.AddRangeAsync(insightsToSave, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);

        return true;
    }

    private static decimal CalculateHealthScore(
        decimal liquidAssets,
        decimal income,
        decimal expenses,
        decimal debt
    )
    {
        decimal monthlyCostTarget = expenses > 0 ? expenses : 1000m;
        decimal emergencyRatio = liquidAssets / (monthlyCostTarget * 6);
        decimal eScore = Math.Min(emergencyRatio * 100, 100);

        decimal savingRatio = income > 0 ? ((income - expenses) / income) : 0;
        decimal sScore =
            savingRatio >= 0.20m ? 100 : (savingRatio > 0 ? (savingRatio / 0.20m) * 100 : 0);

        decimal annualIncomeEstimate = income * 12;
        decimal debtRatio = annualIncomeEstimate > 0 ? (debt / annualIncomeEstimate) : 1;
        decimal dScore = debtRatio <= 0.30m ? 100 : Math.Max(100 - ((debtRatio - 0.30m) * 100), 0);

        const decimal vScore = 80m;

        var score = (0.35m * eScore) + (0.30m * sScore) + (0.25m * dScore) + (0.10m * vScore);
        return Math.Round(score, 2);
    }
}
