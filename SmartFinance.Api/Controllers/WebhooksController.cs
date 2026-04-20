using System.Security.Cryptography;
using System.Text;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartFinance.Application.Ingestion.Commands;

namespace SmartFinance.Api.Controllers;


[AllowAnonymous]
[ApiController]
[Route("api/[controller]")]
public sealed class WebhooksController(IMediator mediator, IConfiguration configuration)
    : ControllerBase
{
    [HttpPost("inbound-email")]
    public async Task<IActionResult> ReceiveInboundEmail(
        [FromQuery] string token,
        [FromForm] IFormCollection form
    )
    {
        var expectedToken = configuration["WebhookSecretToken"];

        if (string.IsNullOrWhiteSpace(token) || token != expectedToken)
        {
            return Unauthorized();
        }

        var from = form["from"].ToString();
        var subject = form["subject"].ToString();
        var body = form["text"].ToString();

        var rawHeaders = form["headers"].ToString();
        var messageId =
            ExtractMessageId(rawHeaders) ?? GenerateFallbackMessageId(from, subject, body);

        if (string.IsNullOrEmpty(from) || string.IsNullOrEmpty(body))
            return BadRequest("Payload incompleto.");

        var command = new ReceiveInboundEmailCommand(messageId, from, subject, body);

        var eventId = await mediator.Send(command);

        if (eventId == null)
            return Ok();

        return Accepted(
            new { Message = "E-mail enfileirado para processamento.", EventId = eventId }
        );
    }

    private static string? ExtractMessageId(string headers)
    {
        if (string.IsNullOrWhiteSpace(headers))
            return null;

        var match = System.Text.RegularExpressions.Regex.Match(
            headers,
            @"Message-ID:\s*<(.+?)>",
            System.Text.RegularExpressions.RegexOptions.IgnoreCase
        );
        return match.Success ? match.Groups[1].Value : null;
    }

    private static string GenerateFallbackMessageId(string from, string subject, string body)
    {
        var input = $"{from}|{subject}|{body}";
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToBase64String(bytes);
    }
}
