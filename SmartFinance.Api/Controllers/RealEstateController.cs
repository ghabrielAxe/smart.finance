using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartFinance.Application.RealEstate.Commands;
using SmartFinance.Application.RealEstate.Queries;

namespace SmartFinance.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class RealEstateController : ControllerBase
{
    private readonly IMediator _mediator;

    public RealEstateController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("contracts")]
    public async Task<IActionResult> CreateContract(
        [FromBody] CreateRealEstateContractCommand command
    )
    {
        var contractId = await _mediator.Send(command);

        return CreatedAtAction(
            nameof(GetDashboard),
            new { contractId = contractId },
            new { Message = "Contrato Imobiliário criado com sucesso.", ContractId = contractId }
        );
    }

    [HttpGet("contracts/{contractId:guid}/dashboard")]
    public async Task<IActionResult> GetDashboard(Guid contractId)
    {
        var result = await _mediator.Send(new GetRealEstateDashboardQuery(contractId));
        return Ok(result);
    }

    [HttpGet("simulate-mortgage")]
    public async Task<IActionResult> SimulateMortgage(
        [FromQuery] decimal principal,
        [FromQuery] string currency,
        [FromQuery] decimal interestRate,
        [FromQuery] int months,
        [FromQuery] DateTime startDate
    )
    {
        var query = new SimulateMortgageQuery(principal, currency, interestRate, months, startDate);
        return Ok(await _mediator.Send(query));
    }

    [HttpPost("contracts/{contractId:guid}/mortgage")]
    public async Task<IActionResult> StartMortgage(
        Guid contractId,
        [FromBody] StartMortgageCommand command
    )
    {
        if (contractId != command.ContractId)
            return BadRequest(new { Error = "ID do contrato diverge do payload." });

        var mortgageId = await _mediator.Send(command);
        return Ok(new { Message = "Financiamento ativado com sucesso.", MortgageId = mortgageId });
    }

    [HttpGet("contracts/{contractId:guid}/mortgage")]
    public async Task<IActionResult> GetMortgageDetails(Guid contractId)
    {
        var result = await _mediator.Send(new GetMortgageDetailsQuery(contractId));
        return Ok(result);
    }
}
