using Hephaestus.Domain.DTOs.Request;
using Hephaestus.Domain.DTOs.Response;
using Hephaestus.Application.Interfaces.Category;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Hephaestus.Controllers;

/// <summary>
/// Controller para gerenciamento de categorias de menu.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,Tenant")]
public class CategoryController : ControllerBase
{
    private readonly ICreateCategoryUseCase _createCategoryUseCase;
    private readonly IGetCategoriesUseCase _getCategoriesUseCase;
    private readonly IGetCategoryByIdUseCase _getCategoryByIdUseCase;
    private readonly IUpdateCategoryUseCase _updateCategoryUseCase;
    private readonly IDeleteCategoryUseCase _deleteCategoryUseCase;
    private readonly ILogger<CategoryController> _logger;

    public CategoryController(
        ICreateCategoryUseCase createCategoryUseCase,
        IGetCategoriesUseCase getCategoriesUseCase,
        IGetCategoryByIdUseCase getCategoryByIdUseCase,
        IUpdateCategoryUseCase updateCategoryUseCase,
        IDeleteCategoryUseCase deleteCategoryUseCase,
        ILogger<CategoryController> logger)
    {
        _createCategoryUseCase = createCategoryUseCase;
        _getCategoriesUseCase = getCategoriesUseCase;
        _getCategoryByIdUseCase = getCategoryByIdUseCase;
        _updateCategoryUseCase = updateCategoryUseCase;
        _deleteCategoryUseCase = deleteCategoryUseCase;
        _logger = logger;
    }

    /// <summary>
    /// Cria uma nova categoria com funcionalidade híbrida.
    /// </summary>
    /// <remarks>
    /// Este endpoint permite criar categorias com comportamento híbrido baseado no role do usuário:
    /// - **Admin**: Cria categorias globais (acessíveis por todos os tenants)
    /// - **Tenant**: Cria categorias locais (específicas da empresa)
    /// 
    /// Requer autenticação com a role **Admin** ou **Tenant**.
    ///
    /// **Exemplo de Request para Admin (Categoria Global):**
    /// ```json
    /// {
    ///   "name": "Bebidas",
    ///   "description": "Categoria global para todas as bebidas",
    ///   "isActive": true
    /// }
    /// ```
    ///
    /// **Exemplo de Request para Tenant (Categoria Local):**
    /// ```json
    /// {
    ///   "name": "Especial da Casa",
    ///   "description": "Categoria específica da empresa",
    ///   "isActive": true
    /// }
    /// ```
    ///
    /// **Exemplo de Response (Status 201 Created):**
    /// ```json
    /// {
    ///   "id": "123e4567-e89b-12d3-a456-426614174001"
    /// }
    /// ```
    /// </remarks>
    /// <param name="request">Dados da categoria a ser criada.</param>
    /// <returns>ID da categoria criada.</returns>
    [HttpPost]
    [SwaggerOperation(Summary = "Cria uma nova categoria", Description = "Cria uma nova categoria com funcionalidade híbrida. Admin cria categorias globais, Tenant cria categorias locais. Requer autenticação com Role=Admin ou Tenant.")]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(object))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ValidationProblemDetails))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryRequest request)
    {
        var id = await _createCategoryUseCase.ExecuteAsync(request, User);
        return CreatedAtAction(nameof(GetCategoryById), new { id }, new { id });
    }

    /// <summary>
    /// Lista as categorias com funcionalidade híbrida.
    /// </summary>
    /// <remarks>
    /// Este endpoint retorna uma lista paginada de categorias híbridas:
    /// - **Para Tenants**: Retorna categorias locais da empresa + todas as categorias globais
    /// - **Para Admins**: Retorna todas as categorias (locais e globais)
    /// 
    /// Requer autenticação com a role **Admin** ou **Tenant**.
    ///
    /// **Exemplo de Response (Status 200 OK):**
    /// ```json
    /// {
    ///   "items": [
    ///     {
    ///       "id": "123e4567-e89b-12d3-a456-426614174001",
    ///       "tenantId": "",
    ///       "name": "Bebidas",
    ///       "description": "Categoria global para todas as bebidas",
    ///       "isActive": true,
    ///       "isGlobal": true,
    ///       "createdAt": "2024-01-01T12:00:00Z",
    ///       "updatedAt": "2024-01-01T12:00:00Z"
    ///     },
    ///     {
    ///       "id": "789e0123-e89b-12d3-a456-426614174003",
    ///       "tenantId": "456e7890-e89b-12d3-a456-426614174002",
    ///       "name": "Especial da Casa",
    ///       "description": "Categoria específica da empresa",
    ///       "isActive": true,
    ///       "isGlobal": false,
    ///       "createdAt": "2024-01-01T12:00:00Z",
    ///       "updatedAt": "2024-01-01T12:00:00Z"
    ///     }
    ///   ],
    ///   "totalCount": 2,
    ///   "pageNumber": 1,
    ///   "pageSize": 20
    /// }
    /// ```
    /// </remarks>
    /// <param name="pageNumber">Número da página para paginação (padrão: 1).</param>
    /// <param name="pageSize">Número de itens por página para paginação (padrão: 20).</param>
    /// <param name="sortBy">Campo para ordenação.</param>
    /// <param name="sortOrder">Direção da ordenação (asc/desc).</param>
    /// <returns>Lista paginada de categorias híbridas.</returns>
    [HttpGet]
    [SwaggerOperation(Summary = "Lista categorias híbridas", Description = "Retorna uma lista paginada de categorias híbridas (locais + globais). Requer autenticação com Role=Admin ou Tenant.")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PagedResult<CategoryResponse>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ProblemDetails))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetCategories(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? sortBy = null,
        [FromQuery] string? sortOrder = "asc")
    {
        var categories = await _getCategoriesUseCase.ExecuteAsync(User, pageNumber, pageSize, sortBy, sortOrder);
        return Ok(categories);
    }

    /// <summary>
    /// Obtém uma categoria específica pelo seu ID.
    /// </summary>
    /// <remarks>
    /// Este endpoint retorna as informações detalhadas de uma categoria específica, desde que pertença ao tenant autenticado.
    /// Requer autenticação com a role **Tenant**.
    ///
    /// **Exemplo de Response (Status 200 OK):**
    /// ```json
    /// {
    ///   "id": "123e4567-e89b-12d3-a456-426614174001",
    ///   "tenantId": "456e7890-e89b-12d3-a456-426614174002",
    ///   "name": "Bebidas",
    ///   "description": "Categoria para todas as bebidas disponíveis",
    ///   "isActive": true,
    ///   "createdAt": "2024-01-01T12:00:00Z",
    ///   "updatedAt": "2024-01-01T12:00:00Z"
    /// }
    /// ```
    /// </remarks>
    /// <param name="id">ID da categoria a ser obtida.</param>
    /// <returns>Detalhes da categoria ou NotFound se não encontrada.</returns>
    [HttpGet("{id}")]
    [SwaggerOperation(Summary = "Obtém categoria por ID", Description = "Retorna detalhes de uma categoria específica para o tenant autenticado. Requer autenticação com Role=Tenant.")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CategoryResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ProblemDetails))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ProblemDetails))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetCategoryById(string id)
    {
        var category = await _getCategoryByIdUseCase.ExecuteAsync(id, User);
        if (category == null)
            return NotFound(new { error = new { code = "NOT_FOUND", message = "Categoria não encontrada" } });
        return Ok(category);
    }

    /// <summary>
    /// Atualiza uma categoria existente.
    /// </summary>
    /// <remarks>
    /// Este endpoint permite que um tenant atualize uma categoria existente.
    /// Requer autenticação com a role **Tenant**.
    ///
    /// **Exemplo de Request:**
    /// ```json
    /// {
    ///   "name": "Bebidas Refrigerantes",
    ///   "description": "Categoria para refrigerantes e bebidas geladas",
    ///   "isActive": true
    /// }
    /// ```
    /// </remarks>
    /// <param name="id">ID da categoria a ser atualizada.</param>
    /// <param name="request">Dados atualizados da categoria.</param>
    /// <returns>Status 204 No Content em caso de sucesso.</returns>
    [HttpPut("{id}")]
    [SwaggerOperation(Summary = "Atualiza uma categoria", Description = "Atualiza uma categoria existente para o tenant autenticado. Requer autenticação com Role=Tenant.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ValidationProblemDetails))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ProblemDetails))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateCategory(string id, [FromBody] UpdateCategoryRequest request)
    {
        await _updateCategoryUseCase.ExecuteAsync(id, request, User);
        return NoContent();
    }

    /// <summary>
    /// Exclui uma categoria.
    /// </summary>
    /// <remarks>
    /// Este endpoint permite que um tenant exclua uma categoria existente.
    /// Requer autenticação com a role **Tenant**.
    /// </remarks>
    /// <param name="id">ID da categoria a ser excluída.</param>
    /// <returns>Status 204 No Content em caso de sucesso.</returns>
    [HttpDelete("{id}")]
    [SwaggerOperation(Summary = "Exclui uma categoria", Description = "Exclui uma categoria para o tenant autenticado. Requer autenticação com Role=Tenant.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ProblemDetails))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ProblemDetails))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteCategory(string id)
    {
        await _deleteCategoryUseCase.ExecuteAsync(id, User);
        return NoContent();
    }
} 
