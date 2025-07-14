using FluentValidation;
using Hephaestus.Application.DTOs.Request;
using Hephaestus.Application.Interfaces.OpenAI;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Hephaestus.Controllers;

/// <summary>
/// Controller para integração com a API da OpenAI.
/// </summary>
[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Admin", Policy = "RequireMfa")]
public class OpenAIController : ControllerBase
{
    private readonly IChatWithOpenAIUseCase _chatWithOpenAIUseCase;
    private readonly ILogger<OpenAIController> _logger;

    /// <summary>
    /// Inicializa uma nova instância do <see cref="OpenAIController"/>.
    /// </summary>
    /// <param name="chatWithOpenAIUseCase">Caso de uso para comunicação com a OpenAI.</param>
    /// <param name="logger">Logger para registro de eventos.</param>
    public OpenAIController(IChatWithOpenAIUseCase chatWithOpenAIUseCase, ILogger<OpenAIController> logger)
    {
        _chatWithOpenAIUseCase = chatWithOpenAIUseCase;
        _logger = logger;
    }

    /// <summary>
    /// Consulta o chat da OpenAI.
    /// </summary>
    /// <remarks>
    /// Exemplo de resposta de sucesso:
    /// ```json
    /// {
    ///   "response": "Olá! Como posso ajudá-lo hoje?",
    ///   "usage": {
    ///     "promptTokens": 10,
    ///     "completionTokens": 15,
    ///     "totalTokens": 25
    ///   }
    /// }
    /// ```
    /// Exemplo de erro de validação:
    /// ```json
    /// {
    ///   "error": {
    ///     "code": "VALIDATION_ERROR",
    ///     "message": "Erro de validação",
    ///     "details": {
    ///       "errors": [
    ///         {
    ///           "field": "Prompt",
    ///           "message": "Prompt é obrigatório."
    ///         }
    ///       ]
    ///     }
    ///   }
    /// }
    /// ```
    /// </remarks>
    /// <param name="request">Dados da consulta à OpenAI.</param>
    /// <returns>Resposta da OpenAI.</returns>
    [HttpPost("chat")]
    [SwaggerOperation(Summary = "Consulta o chat da OpenAI", Description = "Envia um prompt e dados à API da OpenAI e retorna a resposta no formato especificado ('text' ou 'json_object').")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(OpenAIResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(object))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(object))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(object))]
    public async Task<IActionResult> Chat([FromBody] OpenAIRequest request)
    {
        var response = await _chatWithOpenAIUseCase.ExecuteAsync(request);
        return Ok(response);
    }
}