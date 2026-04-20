using Dapper;
using MediatR;
using SmartFinance.Application.Interfaces;

namespace SmartFinance.Application.Transactions.Queries;

public record TransactionListItemDto(
    Guid Id,
    DateTime Date,
    string Description,
    decimal Amount,
    string CategoryName,
    string CategoryColor,
    string AccountName
);

public record GetTransactionsQuery(int Days = 30) : IRequest<IEnumerable<TransactionListItemDto>>;

public class GetTransactionsQueryHandler
    : IRequestHandler<GetTransactionsQuery, IEnumerable<TransactionListItemDto>>
{
    private readonly ISqlConnectionFactory _sqlConnectionFactory;
    private readonly ICurrentUserService _currentUserService;

    public GetTransactionsQueryHandler(
        ISqlConnectionFactory sqlConnectionFactory,
        ICurrentUserService currentUserService
    )
    {
        _sqlConnectionFactory = sqlConnectionFactory;
        _currentUserService = currentUserService;
    }

    public async Task<IEnumerable<TransactionListItemDto>> Handle(
        GetTransactionsQuery request,
        CancellationToken cancellationToken
    )
    {
        using var connection = _sqlConnectionFactory.CreateConnection();

        const string sql =
            @"
            SELECT 
                t.""Id"", 
                t.""Date"", 
                t.""Description"", 
                le.""Amount"", 
                c.""Name"" AS CategoryName, 
                c.""Color"" AS CategoryColor,
                a.""Name"" AS AccountName
            FROM ""Transactions"" t
            JOIN ""LedgerEntries"" le ON t.""Id"" = le.""TransactionId""
            JOIN ""Accounts"" a ON le.""AccountId"" = a.""Id""
            LEFT JOIN ""Categories"" c ON t.""CategoryId"" = c.""Id""
            WHERE t.""UserId"" = @UserId 
              AND le.""Type"" = 1 -- Mostramos o valor debitado da conta
              AND t.""Date"" >= CURRENT_DATE - make_interval(days => @Days)
            ORDER BY t.""Date"" DESC, t.""CreatedAt"" DESC";

        return await connection.QueryAsync<TransactionListItemDto>(
            sql,
            new { UserId = _currentUserService.UserId, Days = request.Days }
        );
    }
}
