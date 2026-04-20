using Dapper;
using MediatR;
using SmartFinance.Application.Interfaces;

namespace SmartFinance.Application.Installments.Queries;

public record UpcomingInstallmentDto(
    Guid InstallmentId,
    string PlanDescription,
    int CurrentNumber,
    int TotalInstallments,
    decimal Amount,
    DateTime DueDate
);

public record GetUpcomingInstallmentsQuery() : IRequest<IEnumerable<UpcomingInstallmentDto>>;

public class GetUpcomingInstallmentsQueryHandler
    : IRequestHandler<GetUpcomingInstallmentsQuery, IEnumerable<UpcomingInstallmentDto>>
{
    private readonly ISqlConnectionFactory _sqlConnectionFactory;
    private readonly ICurrentUserService _currentUserService;

    public GetUpcomingInstallmentsQueryHandler(
        ISqlConnectionFactory sqlConnectionFactory,
        ICurrentUserService currentUserService
    )
    {
        _sqlConnectionFactory = sqlConnectionFactory;
        _currentUserService = currentUserService;
    }

    public async Task<IEnumerable<UpcomingInstallmentDto>> Handle(
        GetUpcomingInstallmentsQuery request,
        CancellationToken cancellationToken
    )
    {
        using var connection = _sqlConnectionFactory.CreateConnection();

        // Traz as parcelas que ainda não foram pagas
        const string sql =
            @"
            SELECT 
                i.""Id"" AS InstallmentId,
                p.""Description"" AS PlanDescription,
                i.""CurrentNumber"",
                p.""TotalInstallments"",
                i.""Amount_Amount"" AS Amount,
                i.""DueDate""
            FROM ""InstallmentPlans"" p
            JOIN ""Installments"" i ON p.""Id"" = i.""InstallmentPlanId""
            WHERE p.""UserId"" = @UserId 
              AND i.""IsPaid"" = false
            ORDER BY i.""DueDate"" ASC, p.""CreatedAt"" DESC;
        ";

        return await connection.QueryAsync<UpcomingInstallmentDto>(
            sql,
            new { UserId = _currentUserService.UserId }
        );
    }
}
