using Dapper;
using MediatR;
using SmartFinance.Application.Interfaces;
using SmartFinance.Domain.Entities;

namespace SmartFinance.Application.Accounts.Queries;

public record AccountListDto(Guid Id, string Name, AccountType Type);

public record GetAccountsQuery() : IRequest<IEnumerable<AccountListDto>>;

public class GetAccountsQueryHandler
    : IRequestHandler<GetAccountsQuery, IEnumerable<AccountListDto>>
{
    private readonly ISqlConnectionFactory _sqlConnectionFactory;
    private readonly ICurrentUserService _currentUserService;

    public GetAccountsQueryHandler(
        ISqlConnectionFactory sqlConnectionFactory,
        ICurrentUserService currentUserService
    )
    {
        _sqlConnectionFactory = sqlConnectionFactory;
        _currentUserService = currentUserService;
    }

    public async Task<IEnumerable<AccountListDto>> Handle(
        GetAccountsQuery request,
        CancellationToken cancellationToken
    )
    {
        using var connection = _sqlConnectionFactory.CreateConnection();

        const string sql =
            @"
            SELECT ""Id"", ""Name"", ""Type"" 
            FROM ""Accounts"" 
            WHERE ""UserId"" = @UserId
            ORDER BY ""Name"";";

        return await connection.QueryAsync<AccountListDto>(
            sql,
            new { UserId = _currentUserService.UserId }
        );
    }
}
