using Hephaestus.Application.Services;
using System.Text.Json;

namespace Hephaestus.Middleware;

/// <summary>
/// Middleware para tratamento global de exceções.
/// </summary>
public class GlobalExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlingMiddleware> _logger;
    private readonly IExceptionHandlerService _exceptionHandler;

    public GlobalExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionHandlingMiddleware> logger,
        IExceptionHandlerService exceptionHandler)
    {
        _next = next;
        _logger = logger;
        _exceptionHandler = exceptionHandler;
    }

    /// <summary>
    /// Processa a requisição e trata exceções globalmente.
    /// </summary>
    /// <param name="context">Contexto da requisição HTTP.</param>
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            // Adiciona timeout para evitar travamentos
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(60));
            
            // Combina o token de timeout com o token de cancelamento da requisição
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cts.Token, context.RequestAborted);
            
            // Executa a requisição com timeout
            await Task.Run(async () => await _next(context), linkedCts.Token);
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Requisição cancelada por timeout");
            context.Response.StatusCode = 408; // Request Timeout
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync("{\"error\":{\"message\":\"Request timeout\",\"code\":\"TIMEOUT_ERROR\",\"type\":\"TimeoutError\"}}");
        }
        catch (Exception exception)
        {
            await HandleExceptionAsync(context, exception);
        }
    }

    /// <summary>
    /// Trata uma exceção e retorna uma resposta HTTP apropriada.
    /// </summary>
    /// <param name="context">Contexto da requisição HTTP.</param>
    /// <param name="exception">Exceção a ser tratada.</param>
    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        _logger.LogError(exception, "Exceção não tratada: {Message}", exception.Message);

        var exceptionInfo = _exceptionHandler.HandleException(exception);

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = exceptionInfo.StatusCode;

        var response = new
        {
            error = new
            {
                code = exceptionInfo.ErrorCode,
                message = exceptionInfo.Message,
                type = exceptionInfo.ErrorType,
                details = exceptionInfo.Details,
                timestamp = DateTime.UtcNow,
                path = context.Request.Path,
                method = context.Request.Method
            }
        };

        var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        });

        await context.Response.WriteAsync(jsonResponse);
    }
} 
