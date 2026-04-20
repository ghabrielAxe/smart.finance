using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartFinance.Application.Intelligence.Commands;

namespace SmartFinance.Api.Controllers;

[AllowAnonymous]
[ApiController]
[Route("api/[controller]")]
public sealed class IntelligenceController(IMediator mediator, IConfiguration configuration)
    : ControllerBase
{
    [HttpPost("run-daily")]
    public async Task<IActionResult> RunDailyJob([FromHeader(Name = "X-Api-Key")] string apiKey)
    {
        var expectedKey = configuration["CronJobApiKey"];
        if (string.IsNullOrWhiteSpace(apiKey) || apiKey != expectedKey)
            return Unauthorized("API Key inválida ou ausente.");

        var ownerIdStr = configuration["OwnerUserId"];
        if (!Guid.TryParse(ownerIdStr, out var systemUserId))
            return StatusCode(
                500,
                "OwnerUserId não configurado corretamente nas variáveis de ambiente."
            );

        var executionDate = DateTime.UtcNow;

        var command = new RunDailyIntelligenceCommand(systemUserId, executionDate);

        await mediator.Send(command);

        return Ok(new { Message = "Processamento de inteligência diário concluído com sucesso." });
    }
}
