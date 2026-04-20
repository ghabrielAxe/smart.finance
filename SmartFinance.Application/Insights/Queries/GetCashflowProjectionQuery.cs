using System.Data;
using Dapper;
using MediatR;
using SmartFinance.Application.Interfaces;
using SmartFinance.Domain.Services;

namespace SmartFinance.Application.Insights.Queries;

public record CashflowProjectionDto(
    DateTime TargetDate,
    decimal ProjectedBalance,
    bool IsAlertState,
    int DaysOfCashLeft
);

public record GetCashflowProjectionQuery(DateTime TargetDate) : IRequest<CashflowProjectionDto>;

public class GetCashflowProjectionQueryHandler
    : IRequestHandler<GetCashflowProjectionQuery, CashflowProjectionDto>
{
    private readonly ISqlConnectionFactory _sqlConnectionFactory;
    private readonly ICashflowProjectionService _projectionService;
    private readonly ICurrentUserService _currentUserService;

    public GetCashflowProjectionQueryHandler(
        ISqlConnectionFactory sqlConnectionFactory,
        ICashflowProjectionService projectionService,
        ICurrentUserService currentUserService
    )
    {
        _sqlConnectionFactory = sqlConnectionFactory;
        _projectionService = projectionService;
        _currentUserService = currentUserService;
    }

    public async Task<CashflowProjectionDto> Handle(
        GetCashflowProjectionQuery request,
        CancellationToken cancellationToken
    )
    {
        using var connection = _sqlConnectionFactory.CreateConnection();

        const string sql =
            @"
            -- A. Saldo Atual (Consideramos apenas a liquidez imediata: Contas Correntes - Type 0)
            SELECT COALESCE(SUM(CASE WHEN le.""Type"" = 0 THEN le.""Amount"" ELSE -le.""Amount"" END), 0)
            FROM ""Accounts"" a
            JOIN ""LedgerEntries"" le ON a.""Id"" = le.""AccountId""
            WHERE a.""Type"" = 0 AND a.""UserId"" = @UserId;

            -- B. Média de Gastos Variáveis (Calcula o 'Burn Rate' Diário baseado nos últimos 30 dias)
            SELECT COALESCE(SUM(le.""Amount"") / 30.0, 0)
            FROM ""Transactions"" t
            JOIN ""LedgerEntries"" le ON t.""Id"" = le.""TransactionId""
            JOIN ""Accounts"" a ON le.""AccountId"" = a.""Id""
            WHERE a.""UserId"" = @UserId 
              AND le.""Type"" = 1 AND a.""Type"" = 0
              AND t.""Date"" >= CURRENT_DATE - INTERVAL '30 days'
              AND t.""InstallmentId"" IS NULL;

            -- C. Eventos Futuros 1: Parcelas da Construtora Pendentes (Até a data alvo)
            SELECT 
                ""DueDate"" AS ""Date"",
                ""AdjustedAmount_Amount"" AS ""Amount"",
                1 AS ""Type"", -- 1 é o Enum CashflowEventType.FixedExpense
                'Parcela Construtora' AS ""Description""
            FROM ""ConstructionInstallments""
            WHERE ""UserId"" = @UserId 
              AND ""IsPaid"" = false 
              AND ""DueDate"" BETWEEN CURRENT_DATE AND @TargetDate

            UNION ALL

            -- D. Eventos Futuros 2: Balões Imobiliários Pendentes
            SELECT 
                ""DueDate"" AS ""Date"",
                ""AdjustedAmount_Amount"" AS ""Amount"",
                1 AS ""Type"",
                ""Description"" AS ""Description""
            FROM ""BalloonPayments""
            WHERE ""UserId"" = @UserId 
              AND ""IsPaid"" = false 
              AND ""DueDate"" BETWEEN CURRENT_DATE AND @TargetDate;
        ";

        using var multi = await connection.QueryMultipleAsync(
            sql,
            new { request.TargetDate, UserId = _currentUserService.UserId }
        );

        var currentBalance = await multi.ReadSingleAsync<decimal>();
        var averageDailyExpense = await multi.ReadSingleAsync<decimal>();

        var futureEvents = (await multi.ReadAsync<CashflowEvent>()).ToList();

        var projectionResult = _projectionService.ProjectBalanceToDate(
            currentBalance: currentBalance,
            targetDate: request.TargetDate,
            events: futureEvents,
            averageDailyVariableExpense: averageDailyExpense
        );

        return new CashflowProjectionDto(
            projectionResult.TargetDate,
            projectionResult.ProjectedBalance,
            projectionResult.IsAlertState,
            projectionResult.DaysOfCashLeft
        );
    }
}
