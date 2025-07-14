using FluentValidation;
using Hephaestus.Application.DTOs.Request;
using Hephaestus.Application.DTOs.Response;
using Hephaestus.Application.Interfaces.Additional;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Hephaestus.Controllers;

/// <summary>
/// Controller para gerenciamento de adicionais.
/// </summary>
[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Tenant")]
public class AdditionalController : ControllerBase
{
    private readonly ICreateAdditionalUseCase _createAdditionalUseCase;
    private readonly IGetAdditionalsUseCase _getAdditionalsUseCase;
    private readonly IGetAdditionalByIdUseCase _getAdditionalByIdUseCase;
    private readonly IUpdateAdditionalUseCase _updateAdditionalUseCase;
    private readonly IDeleteAdditionalUseCase _deleteAdditionalUseCase;
    private readonly ILogger<AdditionalController> _logger;

    /// <summary>
    /// Inicializa uma nova instância do <see cref="AdditionalController"/>.
    /// </summary>
    /// <param name="createAdditionalUseCase">Caso de uso para criação de adicionais.</param>
    /// <param name="getAdditionalsUseCase">Caso de uso para listagem de adicionais.</param>
    /// <param name="getAdditionalByIdUseCase">Caso de uso para obtenção de adicional por ID.</param>
    /// <param name="updateAdditionalUseCase">Caso de uso para atualização de adicionais.</param>
    /// <param name="deleteAdditionalUseCase">Caso de uso para remoção de adicionais.</param>
    /// <param name="logger">Logger para registro de eventos.</param>
    public AdditionalController(
        ICreateAdditionalUseCase createAdditionalUseCase,
        IGetAdditionalsUseCase getAdditionalsUseCase,
        IGetAdditionalByIdUseCase getAdditionalByIdUseCase,
        IUpdateAdditionalUseCase updateAdditionalUseCase,
        IDeleteAdditionalUseCase deleteAdditionalUseCase,
        ILogger<AdditionalController> logger)
    {
        _createAdditionalUseCase = createAdditionalUseCase;
        _getAdditionalsUseCase = getAdditionalsUseCase;
        _getAdditionalByIdUseCase = getAdditionalByIdUseCase;
        _updateAdditionalUseCase = updateAdditionalUseCase;
        _deleteAdditionalUseCase = deleteAdditionalUseCase;
        _logger = logger;
    }

    /// <summary>
    /// Cria um novo adicional.
    /// </summary>
    /// <remarks>
    /// Exemplo de resposta de sucesso:
    /// ```json
    /// {
    ///   "id": "123e4567-e89b-12d3-a456-426614174001"
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
    ///           "field": "Name",
    ///           "message": "Nome é obrigatório."
    ///         }
    ///       ]
    ///     }
    ///   }
    /// }
    /// ```
    /// </remarks>
    /// <param name="request">Dados do adicional a ser criado.</param>
    /// <returns>ID do adicional criado.</returns>
    [HttpPost]
    [SwaggerOperation(Summary = "Cria adicional", Description = "Cria um novo adicional para o tenant. Requer autenticação com Role=Tenant.")]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(string))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(object))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(object))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(object))]
    public async Task<IActionResult> CreateAdditional([FromBody] CreateAdditionalRequest request)
    {
        var tenantId = GetTenantId();
        var id = await _createAdditionalUseCase.ExecuteAsync(request, tenantId);
        return CreatedAtAction(nameof(GetAdditionalById), new { id }, new { id });
    }

    /// <summary>
    /// Lista adicionais do tenant.
    /// </summary>
    /// <remarks>
    /// Exemplo de resposta de sucesso:
    /// ```json
    /// [
    ///   {
    ///     "id": "123e4567-e89b-12d3-a456-426614174001",
    ///     "tenantId": "456e7890-e89b-12d3-a456-426614174002",
    ///     "name": "Queijo Extra",
    ///     "description": "Adicional de queijo",
    ///     "price": 3.50,
    ///     "isAvailable": true
    ///   }
    /// ]
    /// ```
    /// </remarks>
    /// <returns>Lista de adicionais do tenant.</returns>
    [HttpGet]
    [SwaggerOperation(Summary = "Lista adicionais do tenant", Description = "Retorna a lista de adicionais do tenant. Requer autenticação com Role=Tenant.")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<AdditionalResponse>))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(object))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(object))]
    public async Task<IActionResult> GetAdditionals()
    {
        var tenantId = GetTenantId();
        var additionals = await _getAdditionalsUseCase.ExecuteAsync(tenantId);
        return Ok(additionals);
    }

    /// <summary>
    /// Obtém adicional por ID.
    /// </summary>
    /// <remarks>
    /// Exemplo de resposta de sucesso:
    /// ```json
    /// {
    ///   "id": "123e4567-e89b-12d3-a456-426614174001",
    ///   "tenantId": "456e7890-e89b-12d3-a456-426614174002",
    ///   "name": "Queijo Extra",
    ///   "description": "Adicional de queijo",
    ///   "price": 3.50,
    ///   "isAvailable": true
    /// }
    /// ```
    /// Exemplo de erro:
    /// ```json
    /// {
    ///   "error": "Adicional não encontrado."
    /// }
    /// ```
    /// </remarks>
    /// <param name="id">ID do adicional.</param>
    /// <returns>Detalhes do adicional.</returns>
    [HttpGet("{id}")]
    [SwaggerOperation(Summary = "Obtém adicional por ID", Description = "Retorna detalhes de um adicional. Requer autenticação com Role=Tenant.")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AdditionalResponse))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(object))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(object))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(object))]
    public async Task<IActionResult> GetAdditionalById(string id)
    {
        var tenantId = GetTenantId();
        var additional = await _getAdditionalByIdUseCase.ExecuteAsync(id, tenantId);
        return Ok(additional);
    }

    /// <summary>
    /// Atualiza adicional.
    /// </summary>
    /// <remarks>
    /// Exemplo de resposta de sucesso:
    /// ```
    /// Status: 204 No Content
    /// ```
    /// Exemplo de erro:
    /// ```json
    /// {
    ///   "error": "Adicional não encontrado."
    /// }
    /// ```
    /// </remarks>
    /// <param name="id">ID do adicional.</param>
    /// <param name="request">Dados atualizados do adicional.</param>
    /// <returns>Status da atualização.</returns>
    [HttpPut("{id}")]
    [SwaggerOperation(Summary = "Atualiza adicional", Description = "Atualiza um adicional do tenant. Requer autenticação com Role=Tenant.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(object))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(object))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(object))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(object))]
    public async Task<IActionResult> UpdateAdditional(string id, [FromBody] UpdateAdditionalRequest request)
    {
        var tenantId = GetTenantId();
        await _updateAdditionalUseCase.ExecuteAsync(id, request, tenantId);
        return NoContent();
    }

    /// <summary>
    /// Remove adicional.
    /// </summary>
    /// <remarks>
    /// Exemplo de resposta de sucesso:
    /// ```
    /// Status: 204 No Content
    /// ```
    /// Exemplo de erro:
    /// ```json
    /// {
    ///   "error": "Adicional não encontrado."
    /// }
    /// ```
    /// </remarks>
    /// <param name="id">ID do adicional.</param>
    /// <returns>Status da remoção.</returns>
    [HttpDelete("{id}")]
    [SwaggerOperation(Summary = "Remove adicional", Description = "Remove um adicional do tenant. Requer autenticação com Role=Tenant.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(object))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(object))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(object))]
    public async Task<IActionResult> DeleteAdditional(string id)
    {
        var tenantId = GetTenantId();
        await _deleteAdditionalUseCase.ExecuteAsync(id, tenantId);
        return NoContent();
    }

    /// <summary>
    /// Obtém o TenantId do token de autenticação.
    /// </summary>
    /// <returns>TenantId do token.</returns>
    private string GetTenantId()
    {
        var tenantId = User.FindFirst("TenantId")?.Value;
        if (string.IsNullOrEmpty(tenantId))
        {
            _logger.LogWarning("TenantId não encontrado no token para o usuário {UserId}", 
                User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value);
            throw new UnauthorizedAccessException("TenantId não encontrado no token.");
        }
        return tenantId;
    }
}