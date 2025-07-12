using Hephaestus.Application.DTOs.Request;
using Hephaestus.Application.DTOs.Response;
using Hephaestus.Application.Interfaces.Database;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Hephaestus.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Admin", Policy = "RequireMfa")]
public class DatabaseController : ControllerBase
{
    private readonly IExecuteQueryUseCase _executeQueryUseCase;
    private readonly ILogger<DatabaseController> _logger;

    public DatabaseController(IExecuteQueryUseCase executeQueryUseCase, ILogger<DatabaseController> logger)
    {
        _executeQueryUseCase = executeQueryUseCase;
        _logger = logger;
    }

    [HttpPost("execute-query")]
    [SwaggerOperation(Summary = "Executa uma query SQL", Description = "Recebe uma string de query SQL (apenas SELECT) e retorna os resultados.")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ExecuteQueryResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(object))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(object))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(object))]
    public async Task<IActionResult> ExecuteQuery([FromBody] ExecuteQueryRequest request)
    {
        try
        {
            var response = await _executeQueryUseCase.ExecuteAsync(request);
            return Ok(response);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Erro de validação: {Message}", ex.Message);
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao executar query: {Query}", request.Query);
            return StatusCode(500, new { error = ex.Message });
        }
    }
}