using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace SmartFinance.Api.Infrastructure;

public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken
    )
    {
        _logger.LogError(
            exception,
            "Ocorreu uma exceção não tratada: {Message}",
            exception.Message
        );

        var problemDetails = new ProblemDetails { Instance = httpContext.Request.Path };

        // Mapeamento de Exceções para Status HTTP
        switch (exception)
        {
            case ValidationException fluentValidationEx:
                problemDetails.Title = "Erro de Validação";
                problemDetails.Status = StatusCodes.Status400BadRequest;
                problemDetails.Detail = "Um ou mais erros de validação ocorreram.";
                problemDetails.Extensions["errors"] = fluentValidationEx
                    .Errors.GroupBy(e => e.PropertyName, e => e.ErrorMessage)
                    .ToDictionary(
                        failureGroup => failureGroup.Key,
                        failureGroup => failureGroup.ToArray()
                    );
                break;

            case KeyNotFoundException:
                problemDetails.Title = "Recurso Não Encontrado";
                problemDetails.Status = StatusCodes.Status404NotFound;
                problemDetails.Detail = exception.Message;
                break;

            case UnauthorizedAccessException:
                problemDetails.Title = "Acesso Negado";
                problemDetails.Status = StatusCodes.Status403Forbidden;
                problemDetails.Detail = exception.Message;
                break;

            case InvalidOperationException:
                problemDetails.Title = "Operação Inválida";
                problemDetails.Status = StatusCodes.Status400BadRequest; // Erros de regra de negócio (Ex: Equação contábil inválida)
                problemDetails.Detail = exception.Message;
                break;

            default:
                problemDetails.Title = "Erro Interno do Servidor";
                problemDetails.Status = StatusCodes.Status500InternalServerError;
                problemDetails.Detail = "Ocorreu um erro inesperado. Tente novamente mais tarde.";
                break;
        }

        httpContext.Response.StatusCode = problemDetails.Status.Value;

        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true; // Retorna true para avisar o ASP.NET que a exceção já foi tratada
    }
}
