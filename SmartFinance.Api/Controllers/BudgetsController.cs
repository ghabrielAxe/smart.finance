using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartFinance.Application.Budgets.Commands;
using SmartFinance.Application.Budgets.Queries;

namespace SmartFinance.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class BudgetsController : ControllerBase
{
    private readonly IMediator _mediator;

    public BudgetsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary()
    {
        var result = await _mediator.Send(new GetBudgetsSummaryQuery());
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateBudgetCommand command)
    {
        var budgetId = await _mediator.Send(command);
        return Ok(new { Message = "Orçamento criado com sucesso.", BudgetId = budgetId });
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateBudgetCommand command)
    {
        if (id != command.Id)
            return BadRequest(new { Error = "ID da rota não coincide com o ID do payload." });

        await _mediator.Send(command);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _mediator.Send(new DeleteBudgetCommand(id));
        return NoContent();
    }
}
