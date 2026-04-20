using Dapper;
using MediatR;
using SmartFinance.Application.Interfaces;

namespace SmartFinance.Application.Budgets.Queries;

public record BudgetEnvelopeDto(
    Guid BudgetId,
    string CategoryName,
    string CategoryColor,
    decimal Limit,
    decimal Spent,
    decimal Remaining,
    decimal Progress,
    string Status
);

public record GetBudgetsSummaryQuery() : IRequest<IEnumerable<BudgetEnvelopeDto>>;

public class GetBudgetsSummaryQueryHandler
    : IRequestHandler<GetBudgetsSummaryQuery, IEnumerable<BudgetEnvelopeDto>>
{
    private readonly ISqlConnectionFactory _sqlConnectionFactory;
    private readonly ICurrentUserService _currentUserService;

    public GetBudgetsSummaryQueryHandler(
        ISqlConnectionFactory sqlConnectionFactory,
        ICurrentUserService currentUserService
    )
    {
        _sqlConnectionFactory = sqlConnectionFactory;
        _currentUserService = currentUserService;
    }

    public async Task<IEnumerable<BudgetEnvelopeDto>> Handle(
        GetBudgetsSummaryQuery request,
        CancellationToken cancellationToken
    )
    {
        using var connection = _sqlConnectionFactory.CreateConnection();

        const string sql =
            @"
            SELECT 
                b.""Id"" AS BudgetId,
                c.""Name"" AS CategoryName,
                c.""HexColor"" AS CategoryColor,
                b.""MonthlyLimit_Amount"" AS LimitAmount,
                b.""WarningThreshold_Value"" AS WarningThreshold,
                b.""CriticalThreshold_Value"" AS CriticalThreshold,
                COALESCE(SUM(le.""Amount""), 0) AS SpentAmount
            FROM ""Budgets"" b
            JOIN ""Categories"" c ON b.""CategoryId"" = c.""Id""
            LEFT JOIN ""Transactions"" t ON t.""CategoryId"" = c.""Id"" 
                  AND t.""Date"" >= DATE_TRUNC('month', CURRENT_DATE)
                  AND t.""Date"" < DATE_TRUNC('month', CURRENT_DATE) + INTERVAL '1 month'
                  AND t.""UserId"" = @UserId
            LEFT JOIN ""LedgerEntries"" le ON le.""TransactionId"" = t.""Id"" 
                  AND le.""Type"" = 1 
            WHERE b.""UserId"" = @UserId
            GROUP BY b.""Id"", c.""Name"", c.""HexColor"", b.""MonthlyLimit_Amount"", b.""WarningThreshold_Value"", b.""CriticalThreshold_Value""
            ORDER BY c.""Name"";
        ";

        var rawResults = await connection.QueryAsync<dynamic>(
            sql,
            new { UserId = _currentUserService.UserId }
        );

        var envelopes = rawResults.Select(r =>
        {
            decimal limit = r.LimitAmount;
            decimal spent = r.SpentAmount;
            decimal warning = r.WarningThreshold;
            decimal critical = r.CriticalThreshold;

            decimal progress = limit > 0 ? (spent / limit) : 0;
            decimal remaining = limit - spent;

            string status = "success";
            if (progress >= critical)
                status = "critical";
            else if (progress >= warning)
                status = "warning";

            return new BudgetEnvelopeDto(
                (Guid)r.BudgetId,
                (string)r.CategoryName,
                (string)r.CategoryColor,
                limit,
                spent,
                remaining,
                Math.Round(progress, 2),
                status
            );
        });

        return envelopes;
    }
}
