using Dapper;
using MediatR;
using SmartFinance.Application.Interfaces;

namespace SmartFinance.Application.Forecast.Queries;

// O ponto de dados diário para o gráfico do frontend
public record DailyForecastDto(
    DateTime Date,
    decimal ProjectedBalance,
    decimal ExpectedIncome,
    decimal ExpectedFixedExpenses,
    decimal ProjectedVariableBurnRate
);

public record CashflowForecastResult(
    decimal StartingBalance,
    decimal DailyVariableBurnRate,
    decimal ProjectedEndBalance,
    List<DailyForecastDto> Timeline
);

public record GetCashflowForecastQuery(int DaysToProject = 30) : IRequest<CashflowForecastResult>;

file record PendingExpenseDto(DateTime DueDate, decimal Amount);

file record PendingIncomeDto(DateTime DueDate, decimal Amount); 

public sealed class GetCashflowForecastQueryHandler(
    ISqlConnectionFactory sqlFactory,
    ICurrentUserService currentUserService
) : IRequestHandler<GetCashflowForecastQuery, CashflowForecastResult>
{
    public async Task<CashflowForecastResult> Handle(
        GetCashflowForecastQuery request,
        CancellationToken cancellationToken
    )
    {
        var today = DateTime.UtcNow.Date;
        var endDate = today.AddDays(request.DaysToProject);
        var userId = currentUserService.UserId;

        using var connection = sqlFactory.CreateConnection();
        
        const string sql = """
            -- 1. SALDO ATUAL CONSOLIDADO (Apenas contas de Liquidez: Checking/Savings)
            -- Como usamos Partidas Dobradas: Débito = Entrada de Dinheiro, Crédito = Saída
            SELECT 
                COALESCE(SUM(CASE WHEN le."Type" = 0 THEN le."Amount" ELSE 0 END), 0) - 
                COALESCE(SUM(CASE WHEN le."Type" = 1 THEN le."Amount" ELSE 0 END), 0) AS CurrentBalance
            FROM "LedgerEntries" le
            JOIN "Accounts" a ON le."AccountId" = a."Id"
            WHERE a."UserId" = @UserId AND a."Type" IN (0, 1); -- 0=Checking, 1=Savings

            -- 2. DESPESAS FIXAS PENDENTES (Cartão de Crédito + Financiamento Imobiliário)
            SELECT "DueDate", SUM("Amount") AS Amount
            FROM (
                -- Faturas de Cartão
                SELECT i."DueDate", i."Amount_Amount" AS "Amount"
                FROM "Installments" i
                JOIN "InstallmentPlans" p ON i."InstallmentPlanId" = p."Id"
                WHERE p."UserId" = @UserId AND i."IsPaid" = false AND i."DueDate" BETWEEN @Today AND @EndDate
                
                UNION ALL
                
                -- Parcelas da Construtora (Imóvel)
                SELECT c."DueDate", c."AdjustedAmount_Amount" AS "Amount"
                FROM "ConstructionInstallments" c
                JOIN "RealEstateContracts" r ON c."ContractId" = r."Id"
                WHERE r."UserId" = @UserId AND c."IsPaid" = false AND c."DueDate" BETWEEN @Today AND @EndDate
            ) AS PendingFixed
            GROUP BY "DueDate"
            ORDER BY "DueDate";

            -- 3. MÉDIA DIÁRIA VARIÁVEL (Burn Rate Histórico)
            -- Pega todas as despesas (Ledger Crédito) dos últimos 30 dias que NÃO vieram de parcelamentos
            SELECT COALESCE(SUM(le."Amount") / 30.0, 0) AS DailyBurnRate
            FROM "Transactions" t
            JOIN "LedgerEntries" le ON t."Id" = le."TransactionId"
            JOIN "Accounts" a ON le."AccountId" = a."Id"
            WHERE t."UserId" = @UserId 
              AND le."Type" = 1 -- Saída de dinheiro
              AND a."Type" IN (0, 1) -- Saiu da conta corrente/poupança
              AND t."InstallmentId" IS NULL -- Ignora parcelamentos fixos para não duplicar
              AND t."Date" >= @ThirtyDaysAgo;
            """;

        var thirtyDaysAgo = today.AddDays(-30);

        using var multi = await connection.QueryMultipleAsync(
            sql,
            new
            {
                UserId = userId,
                Today = today,
                EndDate = endDate,
                ThirtyDaysAgo = thirtyDaysAgo,
            }
        );

        var currentBalance = await multi.ReadSingleAsync<decimal>();
        var pendingFixedExpenses = (await multi.ReadAsync<PendingExpenseDto>()).ToList();
        var historicalDailyBurn = await multi.ReadSingleAsync<decimal>();

        var pendingIncomes = new List<PendingIncomeDto>();

        var timeline = new List<DailyForecastDto>();
        var runningBalance = currentBalance;

        for (int i = 0; i <= request.DaysToProject; i++)
        {
            var currentDate = today.AddDays(i);

            var fixedExpensesToday = pendingFixedExpenses
                .Where(e => e.DueDate.Date == currentDate)
                .Sum(e => e.Amount);
            var incomeToday = pendingIncomes
                .Where(i => i.DueDate.Date == currentDate)
                .Sum(i => i.Amount);


            var variableBurnToday = (i == 0) ? 0 : historicalDailyBurn;

            runningBalance = runningBalance + incomeToday - fixedExpensesToday - variableBurnToday;

            timeline.Add(
                new DailyForecastDto(
                    Date: currentDate,
                    ProjectedBalance: Math.Round(runningBalance, 2),
                    ExpectedIncome: incomeToday,
                    ExpectedFixedExpenses: fixedExpensesToday,
                    ProjectedVariableBurnRate: Math.Round(variableBurnToday, 2)
                )
            );
        }

        return new CashflowForecastResult(
            StartingBalance: Math.Round(currentBalance, 2),
            DailyVariableBurnRate: Math.Round(historicalDailyBurn, 2),
            ProjectedEndBalance: Math.Round(runningBalance, 2),
            Timeline: timeline
        );
    }
}
