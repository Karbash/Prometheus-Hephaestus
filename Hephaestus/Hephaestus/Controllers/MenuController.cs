using Hephaestus.Domain.DTOs.Request;
using Hephaestus.Application.Interfaces.Menu;
using Hephaestus.Domain.DTOs.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Security.Claims; // Necessary for ClaimTypes

namespace Hephaestus.Controllers;

/// <summary>
/// Controller for managing menu items within a tenant's catalog.
/// This includes creating, listing, retrieving, updating, and deleting menu items.
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
    /// Initializes a new instance of the <see cref="MenuController"/>.
    /// </summary>
    /// <param name="createMenuItemUseCase">Use case for creating new menu items.</param>
    /// <param name="getMenuItemsUseCase">Use case for listing menu items.</param>
    /// <param name="getMenuItemByIdUseCase">Use case for retrieving a menu item by its ID.</param>
    /// <param name="updateMenuItemUseCase">Use case for updating existing menu items.</param>
    /// <param name="deleteMenuItemUseCase">Use case for deleting menu items.</param>
    /// <param name="logger">Logger for recording events and errors.</param>
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
    /// Creates a new menu item for the authenticated tenant.
    /// </summary>
    /// <remarks>
    /// This endpoint allows a tenant to add a new item to their menu.
    /// Authentication with the **Tenant** role is required.
    ///
    /// Example Request:
    /// ```json
    /// {
    ///   "name": "Pizza Calabresa",
    ///   "description": "Pizza de calabresa defumada com cebola e queijo mussarela.",
    ///   "categoryId": "c0d1e2f3-a4b5-c6d7-e8f9-0a1b2c3d4e5f",
    ///   "price": 45.90,
    ///   "isAvailable": true,
    ///   "tagIds": ["massas", "pizzas"],
    ///   "availableAdditionalIds": ["ad001", "ad002"],
    ///   "imageUrl": "[https://example.com/images/pizza-calabresa.jpg](https://example.com/images/pizza-calabresa.jpg)"
    /// }
    /// ```
    ///
    /// Example Success Response (Status 201 Created):
    /// ```json
    /// {
    ///   "id": "123e4567-e89b-12d3-a456-426614174001"
    /// }
    /// ```
    ///
    /// Example Validation Error Response (Status 400 Bad Request):
    /// ```json
    /// {
    ///   "type": "[https://tools.ietf.org/html/rfc7231#section-6.5.1](https://tools.ietf.org/html/rfc7231#section-6.5.1)",
    ///   "title": "One or more validation errors occurred.",
    ///   "status": 400,
    ///   "errors": {
    ///     "Name": [
    ///       "The Name field is required."
    ///     ],
    ///     "Price": [
    ///       "The Price field must be a positive value."
    ///     ]
    ///   }
    /// }
    /// ```
    /// Example Unauthorized Response (Status 401 Unauthorized):
    /// ```
    /// (No response body, just 401 status)
    /// ```
    /// </remarks>
    /// <param name="request">The data for the menu item to be created.</param>
    /// <returns>A <see cref="CreatedAtActionResult"/> containing the ID of the created menu item.</returns>
    [HttpPost]
    [SwaggerOperation(Summary = "Creates a new menu item", Description = "Creates a new menu item for the authenticated tenant. Requires authentication with Role=Tenant.")]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(object))] // Changed to object to match anonymous type { id }
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ValidationProblemDetails))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateMenuItem([FromBody] CreateMenuItemRequest request)
    {
        var id = await _createMenuItemUseCase.ExecuteAsync(request, User);
        return CreatedAtAction(nameof(GetMenuItemById), new { id }, new { id });
    }

    /// <summary>
    /// Lists menu items for the authenticated tenant with pagination and optional filters by tags and categories.
    /// </summary>
    /// <remarks>
    /// This endpoint retrieves a paginated list of menu items belonging to the authenticated tenant.
    /// You can filter by one or more tag IDs (ex: japonesa, fitness) and/or category IDs.
    /// Authentication with the **Tenant** role is required.
    ///
    /// Example Success Response (Status 200 OK):
    /// ```json
    /// {
    ///   "items": [
    ///     {
    ///       "id": "123e4567-e89b-12d3-a456-426614174001",
    ///       "tenantId": "456e7890-e89b-12d3-a456-426614174002",
    ///       "name": "X-Burger",
    ///       "description": "Hamb�rguer com queijo e salada",
    ///       "categoryId": "789e0123-e89b-12d3-a456-426614174003",
    ///       "price": 25.90,
    ///       "isAvailable": true,
    ///       "tagIds": ["tag1", "tag2"],
    ///       "createdAt": "2024-01-01T12:00:00Z"
    ///     },
    ///     {
    ///       "id": "a9b8c7d6-e5f4-3g2h-1i0j-k9l8m7n6o5p4",
    ///       "tenantId": "456e7890-e89b-12d3-a456-426614174002",
    ///       "name": "Refrigerante Cola",
    ///       "description": "Lata de 350ml",
    ///       "categoryId": "c0d1e2f3-a4b5-c6d7-e8f9-0a1b2c3d4e5f",
    ///       "price": 7.00,
    ///       "isAvailable": true,
    ///       "tagIds": ["bebidas"],
    ///       "createdAt": "2024-01-02T10:00:00Z"
    ///     }
    ///   ],
    ///   "totalCount": 2,
    ///   "pageNumber": 1,
    ///   "pageSize": 20
    /// }
    /// ```
    /// Example Request with filters:
    /// GET /api/menu?tagIds=tag1&tagIds=tag2&categoryIds=cat1&pageNumber=1&pageSize=20
    /// Example Bad Request Response (Status 400 Bad Request):
    /// ```json
    /// {
    ///   "type": "[https://tools.ietf.org/html/rfc7231#section-6.5.1](https://tools.ietf.org/html/rfc7231#section-6.5.1)",
    ///   "title": "Bad Request",
    ///   "status": 400,
    ///   "detail": "Page number must be greater than 0."
    /// }
    /// ```
    /// Example Unauthorized Response (Status 401 Unauthorized):
    /// ```
    /// (No response body, just 401 status)
    /// ```
    /// </remarks>
    /// <param name="pageNumber">The page number for pagination (defaults to 1).</param>
    /// <param name="pageSize">The number of items per page for pagination (defaults to 20).</param>
    /// <param name="tagIds">Optional list of tag IDs to filter by type of food (ex: japonesa, fitness).</param>
    /// <param name="categoryIds">Optional list of category IDs to filter by category.</param>
    /// <param name="maxPrice">Preço máximo para filtrar itens do cardápio (opcional).</param>
    /// <param name="promotionActiveNow">Se verdadeiro, retorna apenas itens com promoções ativas no momento (opcional).</param>
    /// <param name="promotionDayOfWeek">Dia da semana para filtrar promoções ativas (0=domingo, ..., 6=sábado, opcional).</param>
    /// <param name="promotionTime">Horário (HH:mm) para filtrar promoções ativas em um horário específico (opcional).</param>
    /// <returns>An <see cref="OkObjectResult"/> containing a paginated list of <see cref="MenuItemResponse"/>.</returns>
    [HttpGet]
    [SwaggerOperation(Summary = "Lista itens do menu", Description = "Retorna uma lista paginada de itens do menu do tenant autenticado. Permite filtrar por tags (tipo de comida), categorias, preço máximo e promoções ativas.")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PagedResult<MenuItemResponse>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ProblemDetails))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetMenuItems(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? sortBy = null,
        [FromQuery] string? sortOrder = "asc",
        [FromQuery] List<string>? tagIds = null,
        [FromQuery] List<string>? categoryIds = null,
        [FromQuery] decimal? maxPrice = null,
        [FromQuery] bool? promotionActiveNow = null,
        [FromQuery] int? promotionDayOfWeek = null,
        [FromQuery] string? promotionTime = null)
    {
        var menuItems = await _getMenuItemsUseCase.ExecuteAsync(User, pageNumber, pageSize, sortBy, sortOrder, tagIds, categoryIds, maxPrice, promotionActiveNow, promotionDayOfWeek, promotionTime);
        return Ok(menuItems);
    }

    /// <summary>
    /// Retrieves a specific menu item by its ID.
    /// </summary>
    /// <remarks>
    /// This endpoint returns the detailed information of a single menu item, provided it belongs to the authenticated tenant.
    /// Authentication with the **Tenant** role is required.
    ///
    /// Example Success Response (Status 200 OK):
    /// ```json
    /// {
    ///   "id": "123e4567-e89b-12d3-a456-426614174001",
    ///   "tenantId": "456e7890-e89b-12d3-a456-426614174002",
    ///   "name": "X-Burger",
    ///   "description": "Hamb�rguer com queijo e salada",
    ///   "categoryId": "789e0123-e89b-12d3-a456-426614174003",
    ///   "price": 25.90,
    ///   "isAvailable": true,
    ///   "tagIds": ["tag1", "tag2"],
    ///   "availableAdditionalIds": ["add1", "add2"],
    ///   "imageUrl": "[https://example.com/images/x-burger.jpg](https://example.com/images/x-burger.jpg)"
    /// }
    /// ```
    ///
    /// Example Bad Request Response (Status 400 Bad Request):
    /// ```json
    /// {
    ///   "type": "[https://tools.ietf.org/html/rfc7231#section-6.5.1](https://tools.ietf.org/html/rfc7231#section-6.5.1)",
    ///   "title": "Bad Request",
    ///   "status": 400,
    ///   "detail": "The provided ID 'invalid-guid' is not a valid GUID."
    /// }
    /// ```
    /// Example Not Found Response (Status 404 Not Found):
    /// ```json
    /// {
    ///   "type": "[https://tools.ietf.org/html/rfc7231#section-6.5.4](https://tools.ietf.org/html/rfc7231#section-6.5.4)",
    ///   "title": "Not Found",
    ///   "status": 404,
    ///   "detail": "MenuItem with ID '99999999-9999-9999-9999-999999999999' not found for the tenant."
    /// }
    /// ```
    /// Example Unauthorized Response (Status 401 Unauthorized):
    /// ```
    /// (No response body, just 401 status)
    /// ```
    /// </remarks>
    /// <param name="id">The **ID (GUID)** of the menu item to retrieve.</param>
    /// <returns>An <see cref="OkObjectResult"/> containing the <see cref="MenuItemResponse"/> or a <see cref="NotFoundResult"/> if the item is not found.</returns>
    [HttpGet("{id}")]
    [SwaggerOperation(Summary = "Gets a menu item by ID", Description = "Returns details of a specific menu item for the authenticated tenant. Requires authentication with Role=Tenant.")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(MenuItemResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ProblemDetails))] // Added for invalid ID
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ProblemDetails))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetMenuItemById(string id)
    {
        var menuItem = await _getMenuItemByIdUseCase.ExecuteAsync(id, User);
        if (menuItem == null)
            return NotFound(new { error = new { code = "NOT_FOUND", message = "Menu item not found" } });
        return Ok(menuItem);
    }

    /// <summary>
    /// Updates an existing menu item.
    /// </summary>
    /// <remarks>
    /// This endpoint allows a tenant to update an existing menu item. Only the fields provided in the request will be updated.
    /// Authentication with the **Tenant** role is required.
    ///
    /// Example Request:
    /// ```json
    /// {
    ///   "name": "X-Burger Deluxe",
    ///   "description": "Hamb�rguer premium com queijo, salada e bacon",
    ///   "categoryId": "789e0123-e89b-12d3-a456-426614174003",
    ///   "price": 32.90,
    ///   "isAvailable": true,
    ///   "tagIds": ["tag1", "tag2", "premium"],
    ///   "availableAdditionalIds": ["add1", "add2", "add3"],
    ///   "imageUrl": "[https://example.com/images/x-burger-deluxe.jpg](https://example.com/images/x-burger-deluxe.jpg)"
    /// }
    /// ```
    ///
    /// Example Success Response (Status 204 No Content):
    /// ```
    /// (No response body, just 204 status)
    /// ```
    ///
    /// Example Validation Error Response (Status 400 Bad Request):
    /// ```json
    /// {
    ///   "type": "[https://tools.ietf.org/html/rfc7231#section-6.5.1](https://tools.ietf.org/html/rfc7231#section-6.5.1)",
    ///   "title": "One or more validation errors occurred.",
    ///   "status": 400,
    ///   "errors": {
    ///     "Price": [
    ///       "The Price field must be a positive value."
    ///     ]
    ///   }
    /// }
    /// ```
    ///
    /// Example Not Found Response (Status 404 Not Found):
    /// ```json
    /// {
    ///   "type": "[https://tools.ietf.org/html/rfc7231#section-6.5.4](https://tools.ietf.org/html/rfc7231#section-6.5.4)",
    ///   "title": "Not Found",
    ///   "status": 404,
    ///   "detail": "MenuItem with ID '99999999-9999-9999-9999-999999999999' not found for the tenant."
    /// }
    /// ```
    ///
    /// Example Unauthorized Response (Status 401 Unauthorized):
    /// ```
    /// (No response body, just 401 status)
    /// ```
    /// </remarks>
    /// <param name="id">The **ID (GUID)** of the menu item to be updated.</param>
    /// <param name="request">The updated data for the menu item.</param>
    /// <returns>A <see cref="NoContentResult"/> indicating successful update.</returns>
    [HttpPut("{id}")]
    [SwaggerOperation(Summary = "Updates a menu item", Description = "Updates an existing menu item for the authenticated tenant. Requires authentication with Role=Tenant.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ValidationProblemDetails))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ProblemDetails))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateMenuItem(string id, [FromBody] UpdateMenuItemRequest request)
    {
        await _updateMenuItemUseCase.ExecuteAsync(id, request, User);
        return NoContent();
    }

    /// <summary>
    /// Deletes a menu item.
    /// </summary>
    /// <remarks>
    /// This endpoint allows a tenant to delete a menu item from their catalog.
    /// Authentication with the **Tenant** role is required.
    ///
    /// Example Success Response (Status 204 No Content):
    /// ```
    /// (No response body, just 204 status)
    /// ```
    ///
    /// Example Not Found Response (Status 404 Not Found):
    /// ```json
    /// {
    ///   "type": "[https://tools.ietf.org/html/rfc7231#section-6.5.4](https://tools.ietf.org/html/rfc7231#section-6.5.4)",
    ///   "title": "Not Found",
    ///   "status": 404,
    ///   "detail": "MenuItem with ID '99999999-9999-9999-9999-999999999999' not found for the tenant."
    /// }
    /// ```
    ///
    /// Example Unauthorized Response (Status 401 Unauthorized):
    /// ```
    /// (No response body, just 401 status)
    /// ```
    /// </remarks>
    /// <param name="id">The **ID (GUID)** of the menu item to be deleted.</param>
    /// <returns>A <see cref="NoContentResult"/> indicating successful deletion.</returns>
    [HttpDelete("{id}")]
    [SwaggerOperation(Summary = "Deletes a menu item", Description = "Deletes a menu item for the authenticated tenant. Requires authentication with Role=Tenant.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ProblemDetails))] // Added for invalid ID
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ProblemDetails))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteMenuItem(string id)
    {
        await _deleteMenuItemUseCase.ExecuteAsync(id, User);
        return NoContent();
    }
}
