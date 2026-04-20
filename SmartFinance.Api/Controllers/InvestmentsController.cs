using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartFinance.Application.Investments.Commands;

namespace SmartFinance.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public sealed class InvestmentsController(IMediator mediator) : ControllerBase
{
    [HttpPost("buy")]
    public async Task<IActionResult> Buy([FromBody] BuyAssetCommand command)
    {
        var tradeId = await mediator.Send(command);
        return Ok(
            new
            {
                Message = "Ativo adquirido e custódia atualizada com sucesso.",
                TradeId = tradeId,
            }
        );
    }

    [HttpPost("sell")]
    public async Task<IActionResult> Sell([FromBody] SellAssetCommand command)
    {
        var tradeId = await mediator.Send(command);
        return Ok(
            new
            {
                Message = "Ativo vendido e PnL (Lucro/Prejuízo) apurado com sucesso.",
                TradeId = tradeId,
            }
        );
    }
}
