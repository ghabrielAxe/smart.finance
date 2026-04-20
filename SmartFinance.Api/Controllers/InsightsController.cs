using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartFinance.Application.Insights.Queries;

namespace SmartFinance.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class InsightsController : ControllerBase
{
    private readonly IMediator _mediator;

    public InsightsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Retorna o Score Financeiro detalhado (0 a 100) baseado em liquidez, dívidas, poupança e estabilidade.
    /// </summary>
    [HttpGet("score")]
    public async Task<IActionResult> GetFinancialScore()
    {
        try
        {
            var result = await _mediator.Send(new GetFinancialHealthScoreQuery());
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(
                500,
                new { Error = "Erro ao calcular o Score Financeiro.", Details = ex.Message }
            );
        }
    }

    /// <summary>
    /// Simula o saldo futuro da conta corrente até uma data específica, considerando o ritmo de gastos atual e contas a pagar (ex: balões).
    /// </summary>
    [HttpGet("cashflow-projection")]
    public async Task<IActionResult> GetCashflowProjection([FromQuery] DateTime targetDate)
    {
        try
        {
            if (targetDate.Date < DateTime.UtcNow.Date)
                return BadRequest(new { Error = "A data alvo da projeção deve estar no futuro." });

            var result = await _mediator.Send(new GetCashflowProjectionQuery(targetDate));
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(
                500,
                new
                {
                    Error = "Erro ao processar a simulação de fluxo de caixa.",
                    Details = ex.Message,
                }
            );
        }
    }
}
