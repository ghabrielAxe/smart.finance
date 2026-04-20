using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartFinance.Application.Installments.Commands;
using SmartFinance.Application.Installments.Queries;

namespace SmartFinance.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class InstallmentPlansController : ControllerBase
{
    private readonly IMediator _mediator;

    public InstallmentPlansController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateInstallmentPlanCommand command)
    {
        var planId = await _mediator.Send(command);
        return Ok(new { Message = "Plano de parcelamento criado com sucesso.", PlanId = planId });
    }

    [HttpGet("upcoming")]
    public async Task<IActionResult> GetUpcomingInstallments()
    {
        var result = await _mediator.Send(new GetUpcomingInstallmentsQuery());
        return Ok(result);
    }
}
