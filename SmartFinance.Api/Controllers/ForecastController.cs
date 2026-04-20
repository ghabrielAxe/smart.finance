using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartFinance.Application.Forecast.Queries;

namespace SmartFinance.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ForecastController : ControllerBase
{
    private readonly IMediator _mediator;

    public ForecastController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("cashflow")]
    public async Task<IActionResult> GetCashflowForecast([FromQuery] int days = 30)
    {
        if (days < 1 || days > 90)
            return BadRequest("O período de previsão deve estar entre 1 e 90 dias.");

        var query = new GetCashflowForecastQuery(days);
        var result = await _mediator.Send(query);

        return Ok(result);
    }
}
