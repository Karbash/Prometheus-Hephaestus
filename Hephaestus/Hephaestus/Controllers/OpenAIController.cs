using Hephaestus.Domain.DTOs.Request;
using Hephaestus.Domain.DTOs.Response; // Certifique-se de que este DTO � usado, se OpenAIResponse � o tipo de retorno
using Hephaestus.Application.Interfaces.OpenAI;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Hephaestus.Controllers;

/// <summary>
/// Controller para integra��o segura com a API da OpenAI, permitindo a comunica��o com modelos de chat.
/// </summary>
[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Admin", Policy = "RequireMfa")]
public class OpenAIController : ControllerBase
{
    private readonly IChatWithOpenAIUseCase _chatWithOpenAIUseCase;
    private readonly ILogger<OpenAIController> _logger;

    /// <summary>
    /// Inicializa uma nova inst�ncia do <see cref="OpenAIController"/>.
    /// </summary>
    /// <param name="chatWithOpenAIUseCase">Caso de uso para comunica��o com a API de chat da OpenAI.</param>
    /// <param name="logger">Logger para registro de eventos e erros.</param>
    public OpenAIController(IChatWithOpenAIUseCase chatWithOpenAIUseCase, ILogger<OpenAIController> logger)
    {
        _chatWithOpenAIUseCase = chatWithOpenAIUseCase;
        _logger = logger;
    }

    /// <summary>
    /// Envia uma consulta (prompt) para o modelo de chat da OpenAI e retorna a resposta.
    /// </summary>
    /// <remarks>
    /// Este endpoint permite interagir com a API de chat da OpenAI, enviando um prompt e, opcionalmente,
    /// especificando o formato de resposta desejado (texto ou JSON).
    /// <br/><br/>
    /// <b>Exemplo de request para resposta em texto:</b>
    /// <code>
    /// {
    ///   "prompt": "Qual a capital da França?",
    ///   "responseFormat": { "type": "text" }
    /// }
    /// </code>
    /// <br/>
    /// <b>Exemplo de request para resposta estruturada:</b>
    /// <code>
    /// {
    ///   "prompt": "Quem é Albert Einstein?",
    ///   "responseFormat": {
    ///     "type": "json_object",
    ///     "biografia": "resumo",
    ///     "nacionalidade": "texto"
    ///   }
    /// }
    /// </code>
    /// <br/>
    /// <b>Exemplo de resposta (campo response como string):</b>
    /// <code>
    /// {
    ///   "responseJson": {
    ///     "response": "A capital da França é Paris."
    ///   }
    /// }
    /// </code>
    /// <br/>
    /// <b>Exemplo de resposta (campo response como objeto JSON):</b>
    /// <code>
    /// {
    ///   "responseJson": {
    ///     "response": {
    ///       "biografia": "Albert Einstein foi um físico teórico alemão...",
    ///       "nacionalidade": "Alemã"
    ///     }
    ///   }
    /// }
    /// </code>
    /// </remarks>
    /// <param name="request">Um objeto <see cref="OpenAIRequest"/> contendo o prompt e configurações adicionais para a consulta.</param>
    /// <returns>Um <see cref="OkObjectResult"/> contendo a resposta da OpenAI em um <see cref="OpenAIResponse"/>.</returns>
    [HttpPost("chat")]
    [SwaggerOperation(Summary = "Consulta o chat da OpenAI", Description = "Envia um prompt e dados � API da OpenAI e retorna a resposta no formato especificado ('text' ou 'json_object'). Requer autentica��o com Role='Admin' e valida��o MFA.")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(OpenAIResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ValidationProblemDetails))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Chat([FromBody] OpenAIRequest request)
    {
        var response = await _chatWithOpenAIUseCase.ExecuteAsync(request);
        return Ok(response);
    }
}
