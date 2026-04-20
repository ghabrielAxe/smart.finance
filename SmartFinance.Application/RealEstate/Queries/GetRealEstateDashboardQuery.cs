using Dapper;
using MediatR;
using SmartFinance.Application.Interfaces;

namespace SmartFinance.Application.RealEstate.Queries;

// DTOs para a Response da API
public record TimelineEventDto(
    Guid Id,
    string Type,
    string Description,
    DateTime DueDate,
    decimal Amount,
    bool IsPaid
);

public record RealEstateDashboardDto(
    Guid ContractId,
    string PropertyName,
    decimal TotalValue,
    decimal TotalPaidToConstructor,
    decimal RemainingBalanceToConstructor,
    DateTime ExpectedDeliveryDate,
    List<TimelineEventDto> UpcomingPayments
);

public record GetRealEstateDashboardQuery(Guid ContractId) : IRequest<RealEstateDashboardDto>;

public class GetRealEstateDashboardQueryHandler
    : IRequestHandler<GetRealEstateDashboardQuery, RealEstateDashboardDto>
{
    private readonly ISqlConnectionFactory _sqlConnectionFactory;
    private readonly ICurrentUserService _currentUserService;

    public GetRealEstateDashboardQueryHandler(
        ISqlConnectionFactory sqlConnectionFactory,
        ICurrentUserService currentUserService
    )
    {
        _sqlConnectionFactory = sqlConnectionFactory;
        _currentUserService = currentUserService;
    }

    public async Task<RealEstateDashboardDto> Handle(
        GetRealEstateDashboardQuery request,
        CancellationToken cancellationToken
    )
    {
        using var connection = _sqlConnectionFactory.CreateConnection();

        const string sqlHeader =
            @"
            SELECT 
                ""Id"" as ContractId, 
                ""PropertyName"", 
                ""PropertyValue_Amount"" as TotalValue, 
                ""ExpectedDeliveryDate""
            FROM ""RealEstateContracts""
            WHERE ""Id"" = @ContractId AND ""UserId"" = @UserId;
        ";

        var header = await connection.QuerySingleOrDefaultAsync<RealEstateDashboardDto>(
            sqlHeader,
            new { request.ContractId, _currentUserService.UserId }
        );

        if (header == null)
            throw new KeyNotFoundException("Contrato não encontrado ou sem permissão de acesso.");

        const string sqlTimeline =
            @"
            SELECT ""Id"", 'Monthly' as Type, 'Parcela Mensal' as Description, ""DueDate"", ""AdjustedAmount_Amount"" as Amount, ""IsPaid""
            FROM ""ConstructionInstallments""
            WHERE ""ContractId"" = @ContractId AND ""UserId"" = @UserId
            
            UNION ALL
            
            SELECT ""Id"", 'Balloon' as Type, ""Description"", ""DueDate"", ""AdjustedAmount_Amount"" as Amount, ""IsPaid""
            FROM ""BalloonPayments""
            WHERE ""ContractId"" = @ContractId AND ""UserId"" = @UserId
            
            ORDER BY ""DueDate"";
        ";

        var timeline = (
            await connection.QueryAsync<TimelineEventDto>(
                sqlTimeline,
                new { request.ContractId, _currentUserService.UserId }
            )
        ).ToList();

        var totalPaid = timeline.Where(t => t.IsPaid).Sum(t => t.Amount);
        var remainingConstructor = timeline.Where(t => !t.IsPaid).Sum(t => t.Amount);

        return header with
        {
            TotalPaidToConstructor = totalPaid,
            RemainingBalanceToConstructor = remainingConstructor,
            UpcomingPayments = timeline,
        };
    }
}
