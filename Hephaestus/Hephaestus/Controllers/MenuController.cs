using FluentValidation;
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
    /// Exemplo de corpo da requisição:
    /// ```json
    /// {
    ///   "name": "Pizza Margherita",
    ///   "description": "Pizza com molho de tomate, manjericão e mussarela",
    ///   "categoryId": "123e4567-e89b-12d3-a456-426614174002",
    ///   "price": 29.90,
    ///   "isAvailable": true,
    ///   "tags": ["pizza", "vegetariana"],
    ///   "availableAdditionalIds": ["123e4567-e89b-12d3-a456-426614174001"],
    ///   "imageUrl": "https://example.com/pizza.jpg"
    /// }
    /// ```
    /// </remarks>
    /// <param name="request">Dados do item do cardápio.</param>
    /// <returns>ID do item criado.</returns>
    [HttpPost]
    [SwaggerOperation(Summary = "Cria item do cardápio", Description = "Cria um novo item do cardápio para o tenant. Requer autenticação com Role=Tenant.")]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(string))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(object))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(object))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(object))]
    public async Task<IActionResult> CreateMenuItem([FromBody] CreateMenuItemRequest request)
    {
        try
        {
            var tenantId = User.FindFirst("TenantId")?.Value;
            if (string.IsNullOrEmpty(tenantId))
                return Unauthorized(new { error = "TenantId não encontrado no token." });

            var id = await _createMenuItemUseCase.ExecuteAsync(request, tenantId);
            return CreatedAtAction(nameof(GetMenuItemById), new { id }, new { id });
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning(ex, "Erro de validação ao criar item do cardápio.");
            return BadRequest(new { errors = ex.Errors });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar item do cardápio.");
            return StatusCode(500, new { error = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Lista itens do cardápio do tenant.
    /// </summary>
    /// <remarks>
    /// Exemplo de resposta:
    /// ```json
    /// [
    ///   {
    ///     "id": "123e4567-e89b-12d3-a456-426614174001",
    ///     "tenantId": "456e7890-e89b-12d3-a456-426614174002",
    ///     "name": "Pizza Margherita",
    ///     "description": "Pizza com molho de tomate, manjericão e mussarela",
    ///     "categoryId": "123e4567-e89b-12d3-a456-426614174002",
    ///     "price": 29.90,
    ///     "isAvailable": true,
    ///     "tags": ["pizza", "vegetariana"],
    ///     "availableAdditionalIds": ["789e0123-e89b-12d3-a456-426614174003"],
    ///     "imageUrl": "https://example.com/pizza.jpg"
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
        try
        {
            var tenantId = User.FindFirst("TenantId")?.Value;
            if (string.IsNullOrEmpty(tenantId))
                return Unauthorized(new { error = "TenantId não encontrado no token." });

            var menuItems = await _getMenuItemsUseCase.ExecuteAsync(tenantId);
            return Ok(menuItems);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao listar itens do cardápio.");
            return StatusCode(500, new { error = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Obtém detalhes de um item do cardápio por ID.
    /// </summary>
    /// <remarks>
    /// Exemplo de resposta:
    /// ```json
    /// {
    ///   "id": "123e4567-e89b-12d3-a456-426614174001",
    ///   "tenantId": "456e7890-e89b-12d3-a456-426614174002",
    ///   "name": "Pizza Margherita",
    ///   "description": "Pizza com molho de tomate, manjericão e mussarela",
    ///   "categoryId": "123e4567-e89b-12d3-a456-426614174002",
    ///   "price": 29.90,
    ///   "isAvailable": true,
    ///   "tags": ["pizza", "vegetariana"],
    ///   "availableAdditionalIds": ["789e0123-e89b-12d3-a456-426614174003"],
    ///   "imageUrl": "https://example.com/pizza.jpg"
    /// }
    /// ```
    /// </remarks>
    /// <param name="id">ID do item do cardápio.</param>
    /// <returns>Detalhes do item.</returns>
    [HttpGet("{id}")]
    [SwaggerOperation(Summary = "Obtém item do cardápio por ID", Description = "Retorna detalhes de um item do cardápio. Requer autenticação com Role=Tenant.")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(MenuItemResponse))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(object))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(object))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(object))]
    public async Task<IActionResult> GetMenuItemById(string id)
    {
        try
        {
            var tenantId = User.FindFirst("TenantId")?.Value;
            if (string.IsNullOrEmpty(tenantId))
                return Unauthorized(new { error = "TenantId não encontrado no token." });

            var menuItem = await _getMenuItemByIdUseCase.ExecuteAsync(id, tenantId);
            return Ok(menuItem);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Item do cardápio {Id} não encontrado.", id);
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter item do cardápio {Id}.", id);
            return StatusCode(500, new { error = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Atualiza um item do cardápio.
    /// </summary>
    /// <remarks>
    /// Exemplo de corpo da requisição:
    /// ```json
    /// {
    ///   "id": "123e4567-e89b-12d3-a456-426614174001",
    ///   "name": "Pizza Margherita Atualizada",
    ///   "description": "Pizza com molho de tomate, manjericão fresco e mussarela de búfala",
    ///   "categoryId": "123e4567-e89b-12d3-a456-426614174002",
    ///   "price": 32.90,
    ///   "isAvailable": true
    /// }
    /// ```
    /// </remarks>
    /// <param name="id">ID do item do cardápio.</param>
    /// <param name="request">Dados atualizados do item.</param>
    /// <returns>Status de sucesso (204 No Content).</returns>
    [HttpPut("{id}")]
    [SwaggerOperation(Summary = "Atualiza item do cardápio", Description = "Atualiza um item do cardápio do tenant. Requer autenticação com Role=Tenant.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(object))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(object))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(object))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(object))]
    public async Task<IActionResult> UpdateMenuItem(string id, [FromBody] UpdateMenuItemRequest request)
    {
        try
        {
            var tenantId = User.FindFirst("TenantId")?.Value;
            if (string.IsNullOrEmpty(tenantId))
                return Unauthorized(new { error = "TenantId não encontrado no token." });

            await _updateMenuItemUseCase.ExecuteAsync(id, request, tenantId);
            return NoContent();
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning(ex, "Erro de validação ao atualizar item do cardápio {Id}.", id);
            return BadRequest(new { errors = ex.Errors });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Erro ao atualizar item do cardápio {Id}: {Message}.", id, ex.Message);
            return BadRequest(new { error = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Item do cardápio {Id} não encontrado.", id);
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar item do cardápio {Id}.", id);
            return StatusCode(500, new { error = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Remove um item do cardápio.
    /// </summary>
    /// <param name="id">ID do item do cardápio.</param>
    /// <returns>Status de sucesso (204 No Content).</returns>
    [HttpDelete("{id}")]
    [SwaggerOperation(Summary = "Remove item do cardápio", Description = "Remove um item do cardápio do tenant. Requer autenticação com Role=Tenant.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(object))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(object))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(object))]
    public async Task<IActionResult> DeleteMenuItem(string id)
    {
        try
        {
            var tenantId = User.FindFirst("TenantId")?.Value;
            if (string.IsNullOrEmpty(tenantId))
                return Unauthorized(new { error = "TenantId não encontrado no token." });

            await _deleteMenuItemUseCase.ExecuteAsync(id, tenantId);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Item do cardápio {Id} não encontrado.", id);
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao remover item do cardápio {Id}.", id);
            return StatusCode(500, new { error = "Erro interno do servidor" });
        }
    }
}