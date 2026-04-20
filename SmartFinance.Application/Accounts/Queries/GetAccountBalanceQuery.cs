using Dapper;
using MediatR;
using SmartFinance.Application.Interfaces;

namespace SmartFinance.Application.Accounts.Queries;

public record AccountBalanceDto(Guid AccountId, string Name, decimal Balance, string Currency);

public record GetAccountBalanceQuery(Guid AccountId) : IRequest<AccountBalanceDto>;

public class GetAccountBalanceQueryHandler
    : IRequestHandler<GetAccountBalanceQuery, AccountBalanceDto>
{
    private readonly ISqlConnectionFactory _sqlConnectionFactory;
    private readonly ICurrentUserService _currentUserService;

    public GetAccountBalanceQueryHandler(
        ISqlConnectionFactory sqlConnectionFactory,
        ICurrentUserService currentUserService
    )
    {
        _sqlConnectionFactory = sqlConnectionFactory;
        _currentUserService = currentUserService;
    }

    public async Task<AccountBalanceDto> Handle(
        GetAccountBalanceQuery request,
        CancellationToken cancellationToken
    )
    {
        using var connection = _sqlConnectionFactory.CreateConnection();

        const string sql =
            @"
            SELECT 
                a.""Id"" AS AccountId,
                a.""Name"",
                COALESCE(SUM(CASE WHEN le.""Type"" = 0 THEN le.""Amount"" ELSE 0 END), 0) - 
                COALESCE(SUM(CASE WHEN le.""Type"" = 1 THEN le.""Amount"" ELSE 0 END), 0) AS Balance,
                MAX(le.""Currency"") AS Currency
            FROM ""Accounts"" a
            LEFT JOIN ""LedgerEntries"" le ON a.""Id"" = le.""AccountId""
            WHERE a.""Id"" = @AccountId AND a.""UserId"" = @UserId
            GROUP BY a.""Id"", a.""Name"";";

        var result = await connection.QuerySingleOrDefaultAsync<AccountBalanceDto>(
            sql,
            new { AccountId = request.AccountId, UserId = _currentUserService.UserId }
        );

        if (result == null)
            throw new KeyNotFoundException(
                "Conta não encontrada ou você não tem permissão para acessá-la."
            );

        return string.IsNullOrEmpty(result.Currency) ? result with { Currency = "BRL" } : result;
    }
}
