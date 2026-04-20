using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartFinance.Application.Transactions.Commands;
using SmartFinance.Application.Transactions.Queries;

namespace SmartFinance.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class TransactionsController : ControllerBase
{
    private readonly IMediator _mediator;

    public TransactionsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<IActionResult> RecordTransaction([FromBody] RecordTransactionCommand command)
    {
        var transactionId = await _mediator.Send(command);

        return Ok(
            new
            {
                Message = "Transação registrada com sucesso no Ledger.",
                TransactionId = transactionId,
            }
        );
    }

    [HttpGet]
    public async Task<IActionResult> GetTransactions([FromQuery] int days = 30)
    {
        return Ok(await _mediator.Send(new GetTransactionsQuery(days)));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateTransaction(
        Guid id,
        [FromBody] UpdateTransactionCommand command
    )
    {
        if (id != command.Id)
            return BadRequest(new { Error = "ID da rota não coincide com o ID do payload." });

        await _mediator.Send(command);
        return NoContent();
    }

    [HttpPatch("{id:guid}/category")]
    public async Task<IActionResult> UpdateCategoryFeedback(
        Guid id,
        [FromBody] UpdateTransactionCategoryCommand command
    )
    {
        if (id != command.TransactionId)
            return BadRequest(new { Error = "ID da rota não coincide com o ID do payload." });

        await _mediator.Send(command);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteTransaction(Guid id)
    {
        await _mediator.Send(new DeleteTransactionCommand(id));
        return NoContent();
    }
}
