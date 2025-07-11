using FluentValidation;
using Hephaestus.Application.DTOs.Request;
using Hephaestus.Application.Interfaces.OpenAI;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Hephaestus.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Admin", Policy = "RequireMfa")]
public class OpenAIController : ControllerBase
{
    private readonly IChatWithOpenAIUseCase _chatWithOpenAIUseCase;
    private readonly ILogger<OpenAIController> _logger;

    public OpenAIController(IChatWithOpenAIUseCase chatWithOpenAIUseCase, ILogger<OpenAIController> logger)
    {
        _chatWithOpenAIUseCase = chatWithOpenAIUseCase;
        _logger = logger;
    }

    [HttpPost("chat")]
    [SwaggerOperation(Summary = "Consulta o chat da OpenAI", Description = "Envia um prompt e dados à API da OpenAI e retorna a resposta no formato especificado ('text' ou 'json_object').")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(OpenAIChatResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(object))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(object))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(object))]
    public async Task<IActionResult> Chat([FromBody] OpenAIChatRequest request)
    {
        try
        {
            var response = await _chatWithOpenAIUseCase.ExecuteAsync(request);
            return Ok(response);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Erro ao consultar OpenAI: {Message}", ex.Message);
            return BadRequest(new { error = ex.Message });
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning(ex, "Erro de validação ao consultar OpenAI.");

            var details = ex.Errors.Select(e => new {
                Field = e.PropertyName,
                Message = e.ErrorMessage
            });

            return BadRequest(new
            {
                status = 400,
                error = "Erro de validação",
                details = details
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro interno ao consultar OpenAI.");
            return StatusCode(500, new { error = "Erro interno do servidor" });
        }
    }
}