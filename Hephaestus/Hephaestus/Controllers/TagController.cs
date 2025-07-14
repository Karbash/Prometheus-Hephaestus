using Hephaestus.Application.DTOs.Request;
using Hephaestus.Application.DTOs.Response;
using Hephaestus.Application.Interfaces.Tag;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Hephaestus.Controllers;

/// <summary>
/// Controller para gerenciamento de tags.
/// </summary>
[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Admin,Tenant")]
public class TagController : ControllerBase
{
    private readonly ICreateTagUseCase _createTagUseCase;
    private readonly IGetAllTagsByTenantUseCase _getAllTagsByTenantUseCase;
    private readonly IDeleteTagUseCase _deleteTagUseCase;
    private readonly ILogger<TagController> _logger;

    /// <summary>
    /// Inicializa uma nova instância do <see cref="TagController"/>.
    /// </summary>
    /// <param name="createTagUseCase">Caso de uso para criação de tags.</param>
    /// <param name="getAllTagsByTenantUseCase">Caso de uso para listagem de tags por tenant.</param>
    /// <param name="deleteTagUseCase">Caso de uso para remoção de tags.</param>
    /// <param name="logger">Logger para registro de eventos.</param>
    public TagController(
        ICreateTagUseCase createTagUseCase,
        IGetAllTagsByTenantUseCase getAllTagsByTenantUseCase,
        IDeleteTagUseCase deleteTagUseCase,
        ILogger<TagController> logger)
    {
        _createTagUseCase = createTagUseCase;
        _getAllTagsByTenantUseCase = getAllTagsByTenantUseCase;
        _deleteTagUseCase = deleteTagUseCase;
        _logger = logger;
    }

    /// <summary>
    /// Cria uma nova tag.
    /// </summary>
    /// <remarks>
    /// Exemplo de resposta de sucesso:
    /// ```json
    /// {
    ///   "id": "123e4567-e89b-12d3-a456-426614174001",
    ///   "tenantId": "456e7890-e89b-12d3-a456-426614174002",
    ///   "name": "Vegetariano"
    /// }
    /// ```
    /// Exemplo de erro:
    /// ```json
    /// {
    ///   "error": "Tag já registrada para este tenant."
    /// }
    /// ```
    /// </remarks>
    /// <param name="request">Dados da tag a ser criada.</param>
    /// <returns>Tag criada.</returns>
    [HttpPost]
    [SwaggerOperation(Summary = "Cria uma nova tag", Description = "Cria uma nova tag para o tenant autenticado. Requer autenticação de administrador ou tenant.")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(TagResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(object))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(object))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(object))]
    public async Task<IActionResult> CreateTag([FromBody] TagRequest request)
    {
        var tagResponse = await _createTagUseCase.ExecuteAsync(request, User);
        return Ok(tagResponse);
    }

    /// <summary>
    /// Lista todas as tags de um tenant.
    /// </summary>
    /// <remarks>
    /// Exemplo de resposta de sucesso:
    /// ```json
    /// [
    ///   {
    ///     "id": "123e4567-e89b-12d3-a456-426614174001",
    ///     "tenantId": "456e7890-e89b-12d3-a456-426614174002",
    ///     "name": "Vegetariano"
    ///   },
    ///   {
    ///     "id": "789e0123-e89b-12d3-a456-426614174003",
    ///     "tenantId": "456e7890-e89b-12d3-a456-426614174002",
    ///     "name": "Sem Glúten"
    ///   }
    /// ]
    /// ```
    /// Exemplo de erro:
    /// ```json
    /// {
    ///   "error": "Tenants só podem listar suas próprias tags."
    /// }
    /// ```
    /// </remarks>
    /// <param name="tenantId">ID do tenant para listar as tags.</param>
    /// <returns>Lista de tags do tenant.</returns>
    [HttpGet]
    [SwaggerOperation(Summary = "Lista todas as tags de um tenant", Description = "Retorna todas as tags associadas a um tenant. Requer autenticação de administrador ou tenant.")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<TagResponse>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(object))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(object))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(object))]
    public async Task<IActionResult> GetAllTagsByTenant([FromQuery] string tenantId)
    {
        var tags = await _getAllTagsByTenantUseCase.ExecuteAsync(tenantId, User);
        return Ok(tags);
    }

    /// <summary>
    /// Exclui uma tag.
    /// </summary>
    /// <remarks>
    /// Exemplo de resposta de sucesso:
    /// ```
    /// Status: 204 No Content
    /// ```
    /// Exemplo de erro:
    /// ```json
    /// {
    ///   "error": "Tag não encontrada."
    /// }
    /// ```
    /// </remarks>
    /// <param name="id">ID da tag a ser excluída.</param>
    /// <returns>Status da exclusão.</returns>
    [HttpDelete("{id}")]
    [SwaggerOperation(Summary = "Exclui uma tag", Description = "Exclui uma tag pelo ID, desde que não esteja associada a itens do cardápio. Requer autenticação de administrador ou tenant.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(object))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(object))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(object))]
    public async Task<IActionResult> DeleteTag(string id)
    {
        await _deleteTagUseCase.ExecuteAsync(id, User);
        return NoContent();
    }
}