using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartFinance.Application.Accounts.Commands;
using SmartFinance.Application.Accounts.Queries;

namespace SmartFinance.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class AccountsController : ControllerBase
{
    private readonly IMediator _mediator;

    public AccountsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<IActionResult> CreateAccount([FromBody] CreateAccountCommand command)
    {
        var accountId = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetBalance), new { id = accountId }, new { Id = accountId });
    }

    [HttpGet]
    public async Task<IActionResult> GetAllAccounts()
    {
        var result = await _mediator.Send(new GetAccountsQuery());
        return Ok(result);
    }

    [HttpGet("{id:guid}/balance")]
    public async Task<IActionResult> GetBalance(Guid id)
    {
        var result = await _mediator.Send(new GetAccountBalanceQuery(id));
        return Ok(result);
    }

    [HttpPost("{id:guid}/close-period")]
    public async Task<IActionResult> ClosePeriod(
        Guid id,
        [FromBody] CloseAccountingPeriodCommand command
    )
    {
        if (id != command.AccountId)
            return BadRequest(new { Error = "ID da rota não coincide com o ID do payload." });

        var reconciliationId = await _mediator.Send(command);
        return Ok(
            new
            {
                Message = "Período fechado e auditado com sucesso.",
                ReconciliationId = reconciliationId,
            }
        );
    }
}
