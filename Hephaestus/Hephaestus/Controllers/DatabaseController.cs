using Hephaestus.Application.DTOs.Request;
using Hephaestus.Application.DTOs.Response;
using Hephaestus.Application.Interfaces.Database;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Hephaestus.Controllers;

/// <summary>
/// Controller para operações de banco de dados.
/// </summary>
[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Admin", Policy = "RequireMfa")]
public class DatabaseController : ControllerBase
{
    private readonly IExecuteQueryUseCase _executeQueryUseCase;
    private readonly ILogger<DatabaseController> _logger;

    /// <summary>
    /// Inicializa uma nova instância do <see cref="DatabaseController"/>.
    /// </summary>
    /// <param name="executeQueryUseCase">Caso de uso para execução de queries.</param>
    /// <param name="logger">Logger para registro de eventos.</param>
    public DatabaseController(IExecuteQueryUseCase executeQueryUseCase, ILogger<DatabaseController> logger)
    {
        _executeQueryUseCase = executeQueryUseCase;
        _logger = logger;
    }

    /// <summary>
    /// Executa uma query SQL de consulta.
    /// </summary>
    /// <remarks>
    /// Exemplo de resposta de sucesso:
    /// ```json
    /// {
    ///   "columns": ["id", "name", "email"],
    ///   "rows": [
    ///     ["1", "João Silva", "joao@email.com"],
    ///     ["2", "Maria Santos", "maria@email.com"]
    ///   ],
    ///   "rowCount": 2
    /// }
    /// ```
    /// Exemplo de erro:
    /// ```json
    /// {
    ///   "error": "Query inválida. Apenas comandos SELECT são permitidos."
    /// }
    /// ```
    /// </remarks>
    /// <param name="request">Query SQL a ser executada.</param>
    /// <returns>Resultados da query.</returns>
    [HttpPost("execute-query")]
    [SwaggerOperation(Summary = "Executa uma query SQL", Description = "Recebe uma string de query SQL (apenas SELECT) e retorna os resultados.")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ExecuteQueryResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(object))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(object))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(object))]
    public async Task<IActionResult> ExecuteQuery([FromBody] ExecuteQueryRequest request)
    {
        var response = await _executeQueryUseCase.ExecuteAsync(request);
        return Ok(response);
    }
}