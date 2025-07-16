using Hephaestus.Domain.DTOs.Request;
using Hephaestus.Domain.DTOs.Response; // Certifique-se de que este DTO é usado, se OpenAIResponse é o tipo de retorno
using Hephaestus.Application.Interfaces.OpenAI;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Hephaestus.Controllers;

/// <summary>
/// Controller para integração segura com a API da OpenAI, permitindo a comunicação com modelos de chat.
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
    /// <param name="chatWithOpenAIUseCase">Caso de uso para comunicação com a API de chat da OpenAI.</param>
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
    /// **Requisitos de Autorização:**
    /// Para acessar este endpoint, o usuário autenticado deve possuir a role **Admin**
    /// e ter passado pela validação de **MFA (Autenticação Multifator)**.
    /// <br/><br/>
    /// **Exemplo de Requisição:**
    /// ```json
    /// {
    ///   "prompt": "Qual a capital da França?",
    ///   "responseFormat": "text"
    /// }
    /// ```
    /// ou
    /// ```json
    /// {
    ///   "prompt": "Me forneça informações sobre a Torre Eiffel em formato JSON com campos 'nome', 'cidade' e 'altura'.",
    ///   "responseFormat": "json_object"
    /// }
    /// ```
    ///
    /// **Exemplo de Resposta de Sucesso (Status 200 OK - 'text' format):**
    /// ```json
    /// {
    ///   "response": "A capital da França é Paris.",
    ///   "usage": {
    ///     "promptTokens": 8,
    ///     "completionTokens": 5,
    ///     "totalTokens": 13
    ///   }
    /// }
    /// ```
    ///
    /// **Exemplo de Resposta de Sucesso (Status 200 OK - 'json_object' format):**
    /// ```json
    /// {
    ///   "response": "{\"nome\": \"Torre Eiffel\", \"cidade\": \"Paris\", \"altura\": \"330 metros\"}",
    ///   "usage": {
    ///     "promptTokens": 25,
    ///     "completionTokens": 20,
    ///     "totalTokens": 45
    ///   }
    /// }
    /// ```
    ///
    /// **Exemplo de Erro de Validação (Status 400 Bad Request):**
    /// ```json
    /// {
    ///   "type": "[https://tools.ietf.org/html/rfc7231#section-6.5.1](https://tools.ietf.org/html/rfc7231#section-6.5.1)",
    ///   "title": "One or more validation errors occurred.",
    ///   "status": 400,
    ///   "errors": {
    ///     "Prompt": [
    ///       "O campo 'Prompt' é obrigatório e não pode ser vazio."
    ///     ]
    ///   }
    /// }
    /// ```
    ///
    /// **Exemplo de Erro de Autorização (Status 401 Unauthorized):**
    /// ```
    /// (Nenhum corpo de resposta, apenas status 401)
    /// ```
    ///
    /// **Exemplo de Erro Interno do Servidor (Status 500 Internal Server Error):**
    /// ```json
    /// {
    ///   "type": "[https://tools.ietf.org/html/rfc7231#section-6.6.1](https://tools.ietf.org/html/rfc7231#section-6.6.1)",
    ///   "title": "Internal Server Error",
    ///   "status": 500,
    ///   "detail": "Ocorreu um erro inesperado ao se comunicar com a API da OpenAI. Detalhes: Serviço indisponível."
    /// }
    /// ```
    /// </remarks>
    /// <param name="request">Um objeto <see cref="OpenAIRequest"/> contendo o prompt e configurações adicionais para a consulta.</param>
    /// <returns>Um <see cref="OkObjectResult"/> contendo a resposta da OpenAI em um <see cref="OpenAIResponse"/>.</returns>
    [HttpPost("chat")]
    [SwaggerOperation(Summary = "Consulta o chat da OpenAI", Description = "Envia um prompt e dados à API da OpenAI e retorna a resposta no formato especificado ('text' ou 'json_object'). Requer autenticação com Role='Admin' e validação MFA.")]
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
