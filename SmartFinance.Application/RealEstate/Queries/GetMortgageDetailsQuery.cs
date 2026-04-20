using Dapper;
using MediatR;
using SmartFinance.Application.Interfaces;

namespace SmartFinance.Application.RealEstate.Queries;

public record MortgageDetailsDto(
    Guid Id,
    string BankName,
    decimal PrincipalAmount,
    decimal AnnualInterestRate,
    int TermMonths,
    DateTime StartDate,
    decimal TotalPaid,
    decimal RemainingBalance,
    IEnumerable<MortgageInstallmentDto> Installments
);

public record GetMortgageDetailsQuery(Guid ContractId) : IRequest<MortgageDetailsDto>;

public class GetMortgageDetailsQueryHandler
    : IRequestHandler<GetMortgageDetailsQuery, MortgageDetailsDto>
{
    private readonly ISqlConnectionFactory _sqlConnectionFactory;
    private readonly ICurrentUserService _currentUserService;

    public GetMortgageDetailsQueryHandler(
        ISqlConnectionFactory sqlConnectionFactory,
        ICurrentUserService currentUserService
    )
    {
        _sqlConnectionFactory = sqlConnectionFactory;
        _currentUserService = currentUserService;
    }

    public async Task<MortgageDetailsDto> Handle(
        GetMortgageDetailsQuery request,
        CancellationToken cancellationToken
    )
    {
        using var connection = _sqlConnectionFactory.CreateConnection();

        const string sqlMortgage =
            @"
            SELECT 
                m.""Id"", m.""BankName"", m.""PrincipalAmount_Amount"" AS PrincipalAmount, 
                m.""AnnualInterestRate_Value"" * 100 AS AnnualInterestRate, 
                m.""TermMonths"", m.""StartDate""
            FROM ""Mortgages"" m
            JOIN ""RealEstateContracts"" r ON m.""ContractId"" = r.""Id""
            WHERE m.""ContractId"" = @ContractId AND r.""UserId"" = @UserId;
        ";

        var mortgage = await connection.QuerySingleOrDefaultAsync<MortgageDetailsDto>(
            sqlMortgage,
            new { request.ContractId, _currentUserService.UserId }
        );

        if (mortgage == null)
            throw new KeyNotFoundException("Financiamento não encontrado ou não iniciado.");

        const string sqlInstallments =
            @"
            SELECT 
                ""InstallmentNumber"" AS Number, ""DueDate"", 
                ""PrincipalAmortization_Amount"" AS Amortization, 
                ""InterestAmount_Amount"" AS Interest, 
                ""TotalAmount_Amount"" AS Total, 
                ""RemainingBalance_Amount"" AS Balance
            FROM ""MortgageInstallments""
            WHERE ""MortgageId"" = @MortgageId
            ORDER BY ""InstallmentNumber"";
        ";

        var installments = await connection.QueryAsync<MortgageInstallmentDto>(
            sqlInstallments,
            new { MortgageId = mortgage.Id }
        );

        return mortgage with
        {
            Installments = installments,
            TotalPaid = installments.Where(i => i.DueDate < DateTime.UtcNow).Sum(i => i.Total),
            RemainingBalance =
                installments
                    .OrderBy(i => i.Number)
                    .LastOrDefault(i => i.DueDate < DateTime.UtcNow)
                    ?.Balance
                ?? mortgage.PrincipalAmount,
        };
    }
}
