using Hephaestus.Application.DTOs.Request;
using Hephaestus.Application.DTOs.Response;
using Hephaestus.Application.Interfaces.Menu;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Hephaestus.Controllers;

/// <summary>
/// Controller para gerenciamento de itens do cardápio.
/// </summary>
[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Tenant")]
public class MenuController : ControllerBase
{
    private readonly ICreateMenuItemUseCase _createMenuItemUseCase;
    private readonly IGetMenuItemsUseCase _getMenuItemsUseCase;
    private readonly IGetMenuItemByIdUseCase _getMenuItemByIdUseCase;
    private readonly IUpdateMenuItemUseCase _updateMenuItemUseCase;
    private readonly IDeleteMenuItemUseCase _deleteMenuItemUseCase;
    private readonly ILogger<MenuController> _logger;

    /// <summary>
    /// Inicializa uma nova instância do <see cref="MenuController"/>.
    /// </summary>
    /// <param name="createMenuItemUseCase">Caso de uso para criação de itens do cardápio.</param>
    /// <param name="getMenuItemsUseCase">Caso de uso para listagem de itens do cardápio.</param>
    /// <param name="getMenuItemByIdUseCase">Caso de uso para obtenção de item do cardápio por ID.</param>
    /// <param name="updateMenuItemUseCase">Caso de uso para atualização de itens do cardápio.</param>
    /// <param name="deleteMenuItemUseCase">Caso de uso para remoção de itens do cardápio.</param>
    /// <param name="logger">Logger para registro de eventos.</param>
    public MenuController(
        ICreateMenuItemUseCase createMenuItemUseCase,
        IGetMenuItemsUseCase getMenuItemsUseCase,
        IGetMenuItemByIdUseCase getMenuItemByIdUseCase,
        IUpdateMenuItemUseCase updateMenuItemUseCase,
        IDeleteMenuItemUseCase deleteMenuItemUseCase,
        ILogger<MenuController> logger)
    {
        _createMenuItemUseCase = createMenuItemUseCase;
        _getMenuItemsUseCase = getMenuItemsUseCase;
        _getMenuItemByIdUseCase = getMenuItemByIdUseCase;
        _updateMenuItemUseCase = updateMenuItemUseCase;
        _deleteMenuItemUseCase = deleteMenuItemUseCase;
        _logger = logger;
    }

    /// <summary>
    /// Cria um novo item do cardápio.
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
    /// <param name="request">Dados do item do cardápio a ser criado.</param>
    /// <returns>ID do item criado.</returns>
    [HttpPost]
    [SwaggerOperation(Summary = "Cria item do cardápio", Description = "Cria um novo item do cardápio para o tenant. Requer autenticação com Role=Tenant.")]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(string))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(object))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(object))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(object))]
    public async Task<IActionResult> CreateMenuItem([FromBody] CreateMenuItemRequest request)
    {
        var tenantId = GetTenantId();
        var id = await _createMenuItemUseCase.ExecuteAsync(request, tenantId);
        return CreatedAtAction(nameof(GetMenuItemById), new { id }, new { id });
    }

    /// <summary>
    /// Lista itens do cardápio.
    /// </summary>
    /// <remarks>
    /// Exemplo de resposta de sucesso:
    /// ```json
    /// [
    ///   {
    ///     "id": "123e4567-e89b-12d3-a456-426614174001",
    ///     "tenantId": "456e7890-e89b-12d3-a456-426614174002",
    ///     "name": "X-Burger",
    ///     "description": "Hambúrguer com queijo e salada",
    ///     "categoryId": "789e0123-e89b-12d3-a456-426614174003",
    ///     "price": 25.90,
    ///     "isAvailable": true,
    ///     "tagIds": ["tag1", "tag2"],
    ///     "availableAdditionalIds": ["add1", "add2"],
    ///     "imageUrl": "https://example.com/burger.jpg"
    ///   }
    /// ]
    /// ```
    /// </remarks>
    /// <returns>Lista de itens do cardápio.</returns>
    [HttpGet]
    [SwaggerOperation(Summary = "Lista itens do cardápio", Description = "Retorna a lista de itens do cardápio do tenant. Requer autenticação com Role=Tenant.")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<MenuItemResponse>))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(object))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(object))]
    public async Task<IActionResult> GetMenuItems()
    {
        var tenantId = GetTenantId();
        var menuItems = await _getMenuItemsUseCase.ExecuteAsync(tenantId);
        return Ok(menuItems);
    }

    /// <summary>
    /// Obtém item do cardápio por ID.
    /// </summary>
    /// <remarks>
    /// Exemplo de resposta de sucesso:
    /// ```json
    /// {
    ///   "id": "123e4567-e89b-12d3-a456-426614174001",
    ///   "tenantId": "456e7890-e89b-12d3-a456-426614174002",
    ///   "name": "X-Burger",
    ///   "description": "Hambúrguer com queijo e salada",
    ///   "categoryId": "789e0123-e89b-12d3-a456-426614174003",
    ///   "price": 25.90,
    ///   "isAvailable": true,
    ///   "tagIds": ["tag1", "tag2"],
    ///   "availableAdditionalIds": ["add1", "add2"],
    ///   "imageUrl": "https://example.com/burger.jpg"
    /// }
    /// ```
    /// Exemplo de erro:
    /// ```json
    /// {
    ///   "error": "Item do cardápio não encontrado."
    /// }
    /// ```
    /// </remarks>
    /// <param name="id">ID do item do cardápio.</param>
    /// <returns>Detalhes do item do cardápio.</returns>
    [HttpGet("{id}")]
    [SwaggerOperation(Summary = "Obtém item do cardápio por ID", Description = "Retorna detalhes de um item do cardápio. Requer autenticação com Role=Tenant.")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(MenuItemResponse))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(object))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(object))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(object))]
    public async Task<IActionResult> GetMenuItemById(string id)
    {
        var tenantId = GetTenantId();
        var menuItem = await _getMenuItemByIdUseCase.ExecuteAsync(id, tenantId);
        return Ok(menuItem);
    }

    /// <summary>
    /// Atualiza item do cardápio.
    /// </summary>
    /// <remarks>
    /// Exemplo de resposta de sucesso:
    /// ```
    /// Status: 204 No Content
    /// ```
    /// Exemplo de erro:
    /// ```json
    /// {
    ///   "error": "Item do cardápio não encontrado."
    /// }
    /// ```
    /// </remarks>
    /// <param name="id">ID do item do cardápio.</param>
    /// <param name="request">Dados atualizados do item do cardápio.</param>
    /// <returns>Status da atualização.</returns>
    [HttpPut("{id}")]
    [SwaggerOperation(Summary = "Atualiza item do cardápio", Description = "Atualiza um item do cardápio do tenant. Requer autenticação com Role=Tenant.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(object))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(object))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(object))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(object))]
    public async Task<IActionResult> UpdateMenuItem(string id, [FromBody] UpdateMenuItemRequest request)
    {
        var tenantId = GetTenantId();
        await _updateMenuItemUseCase.ExecuteAsync(id, request, tenantId);
        return NoContent();
    }

    /// <summary>
    /// Remove item do cardápio.
    /// </summary>
    /// <remarks>
    /// Exemplo de resposta de sucesso:
    /// ```
    /// Status: 204 No Content
    /// ```
    /// Exemplo de erro:
    /// ```json
    /// {
    ///   "error": "Item do cardápio não encontrado."
    /// }
    /// ```
    /// </remarks>
    /// <param name="id">ID do item do cardápio.</param>
    /// <returns>Status da remoção.</returns>
    [HttpDelete("{id}")]
    [SwaggerOperation(Summary = "Remove item do cardápio", Description = "Remove um item do cardápio do tenant. Requer autenticação com Role=Tenant.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(object))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(object))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(object))]
    public async Task<IActionResult> DeleteMenuItem(string id)
    {
        var tenantId = GetTenantId();
        await _deleteMenuItemUseCase.ExecuteAsync(id, tenantId);
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