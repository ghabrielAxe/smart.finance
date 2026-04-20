using Dapper;
using MediatR;
using SmartFinance.Application.Interfaces;

namespace SmartFinance.Application.Categories.Queries;

public record CategoryDto(Guid Id, string Name, Guid? ParentId, string Color, string Icon);

public record GetCategoriesQuery() : IRequest<IEnumerable<CategoryDto>>;

public class GetCategoriesQueryHandler
    : IRequestHandler<GetCategoriesQuery, IEnumerable<CategoryDto>>
{
    private readonly ISqlConnectionFactory _sqlConnectionFactory;
    private readonly ICurrentUserService _currentUserService;

    public GetCategoriesQueryHandler(
        ISqlConnectionFactory sqlConnectionFactory,
        ICurrentUserService currentUserService
    )
    {
        _sqlConnectionFactory = sqlConnectionFactory;
        _currentUserService = currentUserService;
    }

    public async Task<IEnumerable<CategoryDto>> Handle(
        GetCategoriesQuery request,
        CancellationToken cancellationToken
    )
    {
        using var connection = _sqlConnectionFactory.CreateConnection();
        const string sql =
            @"SELECT ""Id"", ""Name"", ""ParentId"", ""Color"", ""Icon"" 
                            FROM ""Categories"" 
                            WHERE ""UserId"" = @UserId 
                            ORDER BY ""ParentId"" NULLS FIRST, ""Name""";

        return await connection.QueryAsync<CategoryDto>(
            sql,
            new { UserId = _currentUserService.UserId }
        );
    }
}
