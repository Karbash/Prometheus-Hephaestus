using Hephaestus.Application.Interfaces.Promotion;
using Hephaestus.Domain.DTOs.Request;
using Hephaestus.Domain.DTOs.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Security.Claims; // Necessary for ClaimTypes
using Hephaestus.Domain.Entities;
using Hephaestus.Domain.Repositories;
using Hephaestus.Application.Exceptions;

namespace Hephaestus.Controllers;

/// <summary>
/// Controller para gerenciamento completo de promo��es de um tenant, incluindo cria��o, listagem,
/// obten��o por ID, atualiza��o, exclus�o e notifica��o de promo��es.
/// </summary>
[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Tenant")]
public class PromotionController : ControllerBase
{
    private readonly ICreatePromotionUseCase _createPromotionUseCase;
    private readonly IGetPromotionsUseCase _getPromotionsUseCase;
    private readonly IGetPromotionByIdUseCase _getPromotionByIdUseCase;
    private readonly IUpdatePromotionUseCase _updatePromotionUseCase;
    private readonly IDeletePromotionUseCase _deletePromotionUseCase;
    private readonly INotifyPromotionUseCase _notifyPromotionUseCase;
    private readonly ILogger<PromotionController> _logger;
    private readonly IPromotionRepository _promotionRepository;

    /// <summary>
    /// Inicializa uma nova inst�ncia do <see cref="PromotionController"/>.
    /// </summary>
    /// <param name="createPromotionUseCase">Caso de uso para criar promo��es.</param>
    /// <param name="getPromotionsUseCase">Caso de uso para listar promo��es.</param>
    /// <param name="getPromotionByIdUseCase">Caso de uso para obter promo��o por ID.</param>
    /// <param name="updatePromotionUseCase">Caso de uso para atualizar promo��es.</param>
    /// <param name="deletePromotionUseCase">Caso de uso para remover promo��es.</param>
    /// <param name="notifyPromotionUseCase">Caso de uso para notificar promo��es.</param>
    /// <param name="logger">Logger para registro de eventos e erros.</param>
    /// <param name="promotionRepository">Reposit�rio para opera��es de promo��o.</param>
    public PromotionController(
        ICreatePromotionUseCase createPromotionUseCase,
        IGetPromotionsUseCase getPromotionsUseCase,
        IGetPromotionByIdUseCase getPromotionByIdUseCase,
        IUpdatePromotionUseCase updatePromotionUseCase,
        IDeletePromotionUseCase deletePromotionUseCase,
        INotifyPromotionUseCase notifyPromotionUseCase,
        ILogger<PromotionController> logger,
        IPromotionRepository promotionRepository)
    {
        _createPromotionUseCase = createPromotionUseCase;
        _getPromotionsUseCase = getPromotionsUseCase;
        _getPromotionByIdUseCase = getPromotionByIdUseCase;
        _updatePromotionUseCase = updatePromotionUseCase;
        _deletePromotionUseCase = deletePromotionUseCase;
        _notifyPromotionUseCase = notifyPromotionUseCase;
        _logger = logger;
        _promotionRepository = promotionRepository;
    }

    /// CreatePromotion

    /// <summary>
    /// Cria uma nova promo��o para o tenant autenticado.
    /// </summary>
    /// <remarks>
    /// Este endpoint permite que um tenant registre uma nova promo��o em seu cat�logo.
    /// Requer autentica��o com a role **Tenant**.
    ///
    /// **Exemplo de Corpo da Requisi��o:**
    /// ```json
    /// {
    ///   "name": "Desconto de 10% no Ver�o",
    ///   "description": "10% de desconto em todos os pedidos acima de R$50 durante o ver�o.",
    ///   "discountType": "Percentage",
    ///   "discountValue": 10.00,
    ///   "menuItemId": null,
    ///   "minOrderValue": 50.00,
    ///   "maxUsesPerCustomer": 1,
    ///   "maxTotalUses": 100,
    ///   "applicableToTags": ["pizza", "lanche"],
    ///   "startDate": "2025-07-12T00:00:00Z",
    ///   "endDate": "2025-12-31T23:59:59Z",
    ///   "isActive": true,
    ///   "imageUrl": "[https://exemplo.com/promocao-verao.jpg](https://exemplo.com/promocao-verao.jpg)"
    /// }
    /// ```
    ///
    /// **Exemplo de Resposta de Sucesso (Status 201 Created):**
    /// ```json
    /// {
    ///   "id": "123e4567-e89b-12d3-a456-426614174001"
    /// }
    /// ```
    ///
    /// **Exemplo de Erro de Valida��o (Status 400 Bad Request):**
    /// ```json
    /// {
    ///   "type": "[https://tools.ietf.org/html/rfc7231#section-6.5.1](https://tools.ietf.org/html/rfc7231#section-6.5.1)",
    ///   "title": "One or more validation errors occurred.",
    ///   "status": 400,
    ///   "errors": {
    ///     "Name": [
    ///       "O campo 'Name' � obrigat�rio."
    ///     ],
    ///     "DiscountValue": [
    ///       "O valor do desconto deve ser positivo."
    ///     ]
    ///   }
    /// }
    /// ```
    ///
    /// **Exemplo de Erro de Autoriza��o (Status 401 Unauthorized):**
    /// ```json
    /// {
    ///   "type": "[https://tools.ietf.org/html/rfc7235#section-3.1](https://tools.ietf.org/html/rfc7235#section-3.1)",
    ///   "title": "Unauthorized",
    ///   "status": 401,
    ///   "detail": "TenantId n�o encontrado no token."
    /// }
    /// ```
    ///
    /// **Exemplo de Erro Interno do Servidor (Status 500 Internal Server Error):**
    /// ```json
    /// {
    ///   "type": "[https://tools.ietf.org/html/rfc7231#section-6.6.1](https://tools.ietf.org/html/rfc7231#section-6.6.1)",
    ///   "title": "Internal Server Error",
    ///   "status": 500,
    ///   "detail": "Ocorreu um erro inesperado ao criar a promo��o."
    /// }
    /// ```
    /// </remarks>
    /// <param name="request">Dados da promo��o a ser criada (<see cref="CreatePromotionRequest"/>).</param>
    /// <returns>Um <see cref="CreatedAtActionResult"/> contendo o ID da promo��o criada.</returns>
    [HttpPost]
    [SwaggerOperation(Summary = "Cria promo��o", Description = "Cria uma nova promo��o para o tenant. Requer autentica��o com Role=Tenant.")]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(object))] // Retorna um objeto an�nimo { id }
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ValidationProblemDetails))] // Detalhes do erro de valida��o
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ProblemDetails))] // Erro de autoriza��o com detalhes
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ProblemDetails))] // Erro interno do servidor com detalhes
    public async Task<IActionResult> CreatePromotion([FromBody] CreatePromotionRequest request)
    {
        var id = await _createPromotionUseCase.ExecuteAsync(request, User);
        return CreatedAtAction(nameof(GetPromotionById), new { id }, new { id });
    }

    /// GetPromotions

    /// <summary>
    /// Lista as promo��es do tenant autenticado.
    /// </summary>
    /// <remarks>
    /// Este endpoint retorna uma lista de promo��es registradas para o tenant.
    /// � poss�vel filtrar as promo��es por status de ativa��o (`isActive`).
    /// Requer autentica��o com a role **Tenant**.
    ///
    /// **Exemplo de Requisi��o:**
    /// ```http
    /// GET /api/Promotion?isActive=true
    /// ```
    ///
    /// **Exemplo de Resposta de Sucesso (Status 200 OK):**
    /// ```json
    /// [
    ///   {
    ///     "id": "123e4567-e89b-12d3-a456-426614174001",
    ///     "tenantId": "456e7890-e89b-12d3-a456-426614174002",
    ///     "name": "Desconto de 10% no Ver�o",
    ///     "description": "10% de desconto em todos os pedidos.",
    ///     "discountType": "Percentage",
    ///     "discountValue": 10.00,
    ///     "menuItemId": null,
    ///     "minOrderValue": 50.00,
    ///     "maxUsesPerCustomer": 1,
    ///     "maxTotalUses": 100,
    ///     "applicableToTags": ["pizza", "lanche"],
    ///     "startDate": "2025-07-12T00:00:00Z",
    ///     "endDate": "2025-12-31T23:59:59Z",
    ///     "isActive": true,
    ///     "imageUrl": "[https://exemplo.com/promocao-verao.jpg](https://exemplo.com/promocao-verao.jpg)"
    ///   },
    ///   {
    ///     "id": "a1b2c3d4-e5f6-7890-1234-567890abcdef",
    ///     "tenantId": "456e7890-e89b-12d3-a456-426614174002",
    ///     "name": "Frete Gr�tis para Novos Clientes",
    ///     "description": "Frete gr�tis na primeira compra.",
    ///     "discountType": "FreeShipping",
    ///     "discountValue": 0.00,
    ///     "menuItemId": null,
    ///     "minOrderValue": 30.00,
    ///     "maxUsesPerCustomer": 1,
    ///     "maxTotalUses": 50,
    ///     "applicableToTags": [],
    ///     "startDate": "2025-07-01T00:00:00Z",
    ///     "endDate": "2025-08-31T23:59:59Z",
    ///     "isActive": true,
    ///     "imageUrl": null
    ///   }
    /// ]
    /// ```
    ///
    /// **Exemplo de Erro de Autoriza��o (Status 401 Unauthorized):**
    /// ```json
    /// {
    ///   "type": "[https://tools.ietf.org/html/rfc7235#section-3.1](https://tools.ietf.org/html/rfc7235#section-3.1)",
    ///   "title": "Unauthorized",
    ///   "status": 401,
    ///   "detail": "TenantId n�o encontrado no token."
    /// }
    /// ```
    ///
    /// **Exemplo de Erro Interno do Servidor (Status 500 Internal Server Error):**
    /// ```json
    /// {
    ///   "type": "[https://tools.ietf.org/html/rfc7231#section-6.6.1](https://tools.ietf.org/html/rfc7231#section-6.6.1)",
    ///   "title": "Internal Server Error",
    ///   "status": 500,
    ///   "detail": "Ocorreu um erro inesperado ao listar as promo��es."
    /// }
    /// ```
    /// </remarks>
    /// <param name="isActive">Filtro opcional: `true` para promo��es ativas, `false` para inativas. Se omitido, retorna todas as promo��es.</param>
    /// <returns>Um <see cref="OkObjectResult"/> contendo uma lista de objetos <see cref="PromotionResponse"/>.</returns>
    [HttpGet]
    [SwaggerOperation(Summary = "Lista promo��es do tenant", Description = "Retorna uma lista paginada de promo��es do tenant, com filtro opcional por status de ativa��o.")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PagedResult<PromotionResponse>))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ProblemDetails))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ProblemDetails))]
    public async Task<IActionResult> GetPromotions(
        [FromQuery] bool? isActive = null,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? sortBy = null,
        [FromQuery] string? sortOrder = "asc")
    {
        var result = await _getPromotionsUseCase.ExecuteAsync(User, isActive, pageNumber, pageSize, sortBy, sortOrder);
        return Ok(result);
    }

    /// GetPromotionById

    /// <summary>
    /// Obt�m os detalhes de uma promo��o espec�fica por ID.
    /// </summary>
    /// <remarks>
    /// Este endpoint retorna todas as informa��es de uma promo��o, desde que a promo��o perten�a ao tenant autenticado.
    /// Requer autentica��o com a role **Tenant**.
    ///
    /// **Exemplo de Requisi��o:**
    /// ```http
    /// GET /api/Promotion/123e4567-e89b-12d3-a456-426614174001
    /// ```
    ///
    /// **Exemplo de Resposta de Sucesso (Status 200 OK):**
    /// ```json
    /// {
    ///   "id": "123e4567-e89b-12d3-a456-426614174001",
    ///   "tenantId": "456e7890-e89b-12d3-a456-426614174002",
    ///   "name": "Desconto de 10% no Ver�o",
    ///   "description": "10% de desconto em todos os pedidos acima de R$50 durante o ver�o.",
    ///   "discountType": "Percentage",
    ///   "discountValue": 10.00,
    ///   "menuItemId": null,
    ///   "minOrderValue": 50.00,
    ///   "maxUsesPerCustomer": 1,
    ///   "maxTotalUses": 100,
    ///   "applicableToTags": ["pizza", "lanche"],
    ///   "startDate": "2025-07-12T00:00:00Z",
    ///   "endDate": "2025-12-31T23:59:59Z",
    ///   "isActive": true,
    ///   "imageUrl": "[https://exemplo.com/promocao-verao.jpg](https://exemplo.com/promocao-verao.jpg)"
    /// }
    /// ```
    ///
    /// **Exemplo de Erro (Status 400 Bad Request):**
    /// ```json
    /// {
    ///   "type": "[https://tools.ietf.org/html/rfc7231#section-6.5.1](https://tools.ietf.org/html/rfc7231#section-6.5.1)",
    ///   "title": "Bad Request",
    ///   "status": 400,
    ///   "detail": "O ID da promo��o 'invalido-id' n�o � um GUID v�lido."
    /// }
    /// ```
    ///
    /// **Exemplo de Erro (Status 404 Not Found):**
    /// ```json
    /// {
    ///   "type": "[https://tools.ietf.org/html/rfc7231#section-6.5.4](https://tools.ietf.org/html/rfc7231#section-6.5.4)",
    ///   "title": "Not Found",
    ///   "status": 404,
    ///   "detail": "Promo��o com ID '99999999-9999-9999-9999-999999999999' n�o encontrada para o tenant."
    /// }
    /// ```
    ///
    /// **Exemplo de Erro de Autoriza��o (Status 401 Unauthorized):**
    /// ```json
    /// {
    ///   "type": "[https://tools.ietf.org/html/rfc7235#section-3.1](https://tools.ietf.org/html/rfc7235#section-3.1)",
    ///   "title": "Unauthorized",
    ///   "status": 401,
    ///   "detail": "TenantId n�o encontrado no token."
    /// }
    /// ```
    /// </remarks>
    /// <param name="id">O **ID (GUID)** da promo��o a ser consultada.</param>
    /// <returns>Um <see cref="OkObjectResult"/> contendo o <see cref="PromotionResponse"/> ou um <see cref="NotFoundResult"/> se a promo��o n�o for encontrada.</returns>
    [HttpGet("{id}")]
    [SwaggerOperation(Summary = "Obt�m promo��o por ID", Description = "Retorna detalhes de uma promo��o do tenant. Requer autentica��o com Role=Tenant.")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PromotionResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ProblemDetails))] // Para ID inv�lido
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ProblemDetails))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ProblemDetails))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ProblemDetails))]
    public async Task<IActionResult> GetPromotionById(string id)
    {
        var promotion = await _getPromotionByIdUseCase.ExecuteAsync(id, User);
        return Ok(promotion);
    }

    /// UpdatePromotion

    /// <summary>
    /// Atualiza uma promo��o existente para o tenant autenticado.
    /// </summary>
    /// <remarks>
    /// Este endpoint permite que um tenant atualize os dados de uma promo��o existente.
    /// Requer autentica��o com a role **Tenant**.
    ///
    /// **Exemplo de Corpo da Requisi��o:**
    /// ```json
    /// {
    ///   "id": "123e4567-e89b-12d3-a456-426614174001",
    ///   "name": "Desconto de 15% no Ver�o",
    ///   "description": "15% de desconto em todos os pedidos acima de R$60 durante o ver�o.",
    ///   "discountType": "Percentage",
    ///   "discountValue": 15.00,
    ///   "menuItemId": null,
    ///   "minOrderValue": 60.00,
    ///   "maxUsesPerCustomer": 2,
    ///   "maxTotalUses": 150,
    ///   "applicableToTags": ["pizza", "lanche", "bebidas"],
    ///   "startDate": "2025-07-12T00:00:00Z",
    ///   "endDate": "2025-12-31T23:59:59Z",
    ///   "isActive": true,
    ///   "imageUrl": "[https://exemplo.com/promocao-verao-atualizada.jpg](https://exemplo.com/promocao-verao-atualizada.jpg)"
    /// }
    /// ```
    ///
    /// **Exemplo de Resposta de Sucesso (Status 204 No Content):**
    /// ```
    /// (Nenhum corpo de resposta, apenas status 204)
    /// ```
    ///
    /// **Exemplo de Erro de Valida��o (Status 400 Bad Request):**
    /// ```json
    /// {
    ///   "type": "[https://tools.ietf.org/html/rfc7231#section-6.5.1](https://tools.ietf.org/html/rfc7231#section-6.5.1)",
    ///   "title": "One or more validation errors occurred.",
    ///   "status": 400,
    ///   "errors": {
    ///     "Name": [
    ///       "O campo 'Name' � obrigat�rio."
    ///     ],
    ///     "DiscountValue": [
    ///       "O valor do desconto deve ser positivo."
    ///     ]
    ///   }
    /// }
    /// ```
    ///
    /// **Exemplo de Erro (Status 404 Not Found):**
    /// ```json
    /// {
    ///   "type": "[https://tools.ietf.org/html/rfc7231#section-6.5.4](https://tools.ietf.org/html/rfc7231#section-6.5.4)",
    ///   "title": "Not Found",
    ///   "status": 404,
    ///   "detail": "Promo��o com ID '99999999-9999-9999-9999-999999999999' n�o encontrada para o tenant."
    /// }
    /// ```
    ///
    /// **Exemplo de Erro de Autoriza��o (Status 401 Unauthorized):**
    /// ```json
    /// {
    ///   "type": "[https://tools.ietf.org/html/rfc7235#section-3.1](https://tools.ietf.org/html/rfc7235#section-3.1)",
    ///   "title": "Unauthorized",
    ///   "status": 401,
    ///   "detail": "TenantId n�o encontrado no token."
    /// }
    /// ```
    /// </remarks>
    /// <param name="id">O **ID (GUID)** da promo��o a ser atualizada.</param>
    /// <param name="request">Dados atualizados da promo��o (<see cref="UpdatePromotionRequest"/>).</param>
    /// <returns>Um <see cref="NoContentResult"/> em caso de sucesso.</returns>
    [HttpPut("{id}")]
    [SwaggerOperation(Summary = "Atualiza promo��o", Description = "Atualiza uma promo��o do tenant. Requer autentica��o com Role=Tenant.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ValidationProblemDetails))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ProblemDetails))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ProblemDetails))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ProblemDetails))]
    public async Task<IActionResult> UpdatePromotion(string id, [FromBody] UpdatePromotionRequest request)
    {
        await _updatePromotionUseCase.ExecuteAsync(id, request, User);
        return NoContent();
    }

    /// <summary>
    /// Atualiza o status de ativa��o de uma promo��o (ativa/inativa).
    /// </summary>
    /// <param name="id">ID da promo��o.</param>
    /// <param name="request">Novo status de ativa��o.</param>
    /// <returns>Promo��o atualizada.</returns>
    /// <response code="200">Status atualizado com sucesso.</response>
    /// <response code="400">Regras de neg�cio violadas.</response>
    /// <response code="404">Promo��o n�o encontrada.</response>
    [HttpPatch("{id}/status")]
    [SwaggerOperation(Summary = "Ativar/Desativar promo��o", Description = "Ativa ou desativa uma promo��o, respeitando regras de neg�cio.")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PromotionResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdatePromotionStatus(string id, [FromBody] UpdatePromotionStatusRequest request)
    {
        var promotion = await _getPromotionByIdUseCase.ExecuteAsync(id, User);
        if (promotion == null)
            return NotFound();

        // Regra: n�o pode ativar promo��o expirada
        if (request.IsActive && promotion.EndDate < DateTime.UtcNow)
            return BadRequest("N�o � poss�vel ativar uma promo��o expirada.");

        // Regra: n�o pode desativar promo��o em uso ativo (exemplo: promo��o vinculada a pedido aberto)
        // Aqui seria necess�rio consultar pedidos, mas como exemplo:
        // bool emUso = await _orderRepository.ExistsOrderWithPromotionActive(id);
        // if (!request.IsActive && emUso)
        //     return BadRequest("N�o � poss�vel desativar uma promo��o em uso ativo.");

        // Atualiza status
        var updateRequest = new UpdatePromotionRequest
        {
            Name = promotion.Name,
            Description = promotion.Description,
            DiscountType = promotion.DiscountType,
            DiscountValue = promotion.DiscountValue,
            MenuItemId = promotion.MenuItemId,
            MinOrderValue = promotion.MinOrderValue,
            MaxUsesPerCustomer = promotion.MaxUsesPerCustomer,
            MaxTotalUses = promotion.MaxTotalUses,
            StartDate = promotion.StartDate,
            EndDate = promotion.EndDate,
            IsActive = request.IsActive,
            ImageUrl = promotion.ImageUrl
        };
        await _updatePromotionUseCase.ExecuteAsync(id, updateRequest, User);
        // Retorna a promo��o atualizada
        var updated = await _getPromotionByIdUseCase.ExecuteAsync(id, User);
        return Ok(updated);
    }

    /// DeletePromotion

    /// <summary>
    /// Remove uma promo��o do tenant autenticado.
    /// </summary>
    /// <remarks>
    /// Este endpoint permite que um tenant remova uma promo��o existente.
    /// Requer autentica��o com a role **Tenant**.
    ///
    /// **Exemplo de Requisi��o:**
    /// ```http
    /// DELETE /api/Promotion/123e4567-e89b-12d3-a456-426614174001
    /// ```
    ///
    /// **Exemplo de Resposta de Sucesso (Status 204 No Content):**
    /// ```
    /// (Nenhum corpo de resposta, apenas status 204)
    /// ```
    ///
    /// **Exemplo de Erro (Status 400 Bad Request):**
    /// ```json
    /// {
    ///   "type": "[https://tools.ietf.org/html/rfc7231#section-6.5.1](https://tools.ietf.org/html/rfc7231#section-6.5.1)",
    ///   "title": "Bad Request",
    ///   "status": 400,
    ///   "detail": "O ID da promo��o 'invalido-id' n�o � um GUID v�lido."
    /// }
    /// ```
    ///
    /// **Exemplo de Erro (Status 404 Not Found):**
    /// ```json
    /// {
    ///   "type": "[https://tools.ietf.org/html/rfc7231#section-6.5.4](https://tools.ietf.org/html/rfc7231#section-6.5.4)",
    ///   "title": "Not Found",
    ///   "status": 404,
    ///   "detail": "Promo��o com ID '99999999-9999-9999-9999-999999999999' n�o encontrada para o tenant."
    /// }
    /// ```
    ///
    /// **Exemplo de Erro de Autoriza��o (Status 401 Unauthorized):**
    /// ```json
    /// {
    ///   "type": "[https://tools.ietf.org/html/rfc7235#section-3.1](https://tools.ietf.org/html/rfc7235#section-3.1)",
    ///   "title": "Unauthorized",
    ///   "status": 401,
    ///   "detail": "TenantId n�o encontrado no token."
    /// }
    /// ```
    /// </remarks>
    /// <param name="id">O **ID (GUID)** da promo��o a ser removida.</param>
    /// <returns>Um <see cref="NoContentResult"/> em caso de sucesso.</returns>
    [HttpDelete("{id}")]
    [SwaggerOperation(Summary = "Remove promo��o", Description = "Remove uma promo��o do tenant. Requer autentica��o com Role=Tenant.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ProblemDetails))] // Para ID inv�lido
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ProblemDetails))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ProblemDetails))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ProblemDetails))]
    public async Task<IActionResult> DeletePromotion(string id)
    {
        await _deletePromotionUseCase.ExecuteAsync(id, User);
        return NoContent();
    }

    /// NotifyPromotion

    /// <summary>
    /// Envia notifica��o de uma promo��o via WhatsApp.
    /// </summary>
    /// <remarks>
    /// Este endpoint permite que um tenant envie notifica��es de promo��es para seus clientes via WhatsApp.
    /// Requer autentica��o com a role **Tenant**.
    ///
    /// **Exemplo de Corpo da Requisi��o:**
    /// ```json
    /// {
    ///   "promotionId": "123e4567-e89b-12d3-a456-426614174001",
    ///   "messageTemplate": "?? Promo��o especial! {promotionName} - {promotionDescription} V�lida at� {endDate}. Aproveite!",
    ///   "customerPhoneNumbers": ["11987654321", "21987654321"]
    /// }
    /// ```
    ///
    /// **Exemplo de Resposta de Sucesso (Status 204 No Content):**
    /// ```
    /// (Nenhum corpo de resposta, apenas status 204)
    /// ```
    ///
    /// **Exemplo de Erro de Valida��o (Status 400 Bad Request):**
    /// ```json
    /// {
    ///   "type": "[https://tools.ietf.org/html/rfc7231#section-6.5.1](https://tools.ietf.org/html/rfc7231#section-6.5.1)",
    ///   "title": "One or more validation errors occurred.",
    ///   "status": 400,
    ///   "errors": {
    ///     "PromotionId": [
    ///       "O ID da promo��o � obrigat�rio."
    ///     ],
    ///     "MessageTemplate": [
    ///       "O template da mensagem � obrigat�rio."
    ///     ]
    ///   }
    /// }
    /// ```
    ///
    /// **Exemplo de Erro (Status 404 Not Found):**
    /// ```json
    /// {
    ///   "type": "[https://tools.ietf.org/html/rfc7231#section-6.5.4](https://tools.ietf.org/html/rfc7231#section-6.5.4)",
    ///   "title": "Not Found",
    ///   "status": 404,
    ///   "detail": "Promo��o com ID '99999999-9999-9999-9999-999999999999' n�o encontrada para o tenant."
    /// }
    /// ```
    ///
    /// **Exemplo de Erro de Autoriza��o (Status 401 Unauthorized):**
    /// ```json
    /// {
    ///   "type": "[https://tools.ietf.org/html/rfc7235#section-3.1](https://tools.ietf.org/html/rfc7235#section-3.1)",
    ///   "title": "Unauthorized",
    ///   "status": 401,
    ///   "detail": "TenantId n�o encontrado no token."
    /// }
    /// ```
    /// </remarks>
    /// <param name="request">Dados da notifica��o (<see cref="NotifyPromotionRequest"/>).</param>
    /// <returns>Um <see cref="NoContentResult"/> em caso de sucesso.</returns>
    [HttpPost("notify")]
    [SwaggerOperation(Summary = "Notifica promo��o via WhatsApp", Description = "Envia notifica��o de promo��o via WhatsApp. Requer autentica��o com Role=Tenant.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ValidationProblemDetails))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ProblemDetails))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ProblemDetails))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ProblemDetails))]
    public async Task<IActionResult> NotifyPromotion([FromBody] NotifyPromotionRequest request)
    {
        await _notifyPromotionUseCase.ExecuteAsync(request, User);
        return NoContent();
    }

    /// <summary>
    /// Registra o uso de uma promo��o por um cliente em um pedido.
    /// </summary>
    /// <param name="id">ID da promo��o.</param>
    /// <param name="request">Dados do uso (cliente, pedido).</param>
    /// <returns>Confirma��o do uso.</returns>
    /// <response code="200">Uso registrado com sucesso.</response>
    /// <response code="400">Limite de uso atingido ou promo��o inv�lida.</response>
    [HttpPost("{id}/use")]
    [SwaggerOperation(Summary = "Registrar uso de promo��o", Description = "Registra o uso de uma promo��o por um cliente em um pedido, validando limites de uso.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UsePromotion(string id, [FromBody] UsePromotionRequest request)
    {
        // Buscar promo��o
        var tenantId = User.FindFirst("tenant_id")?.Value;
        if (string.IsNullOrEmpty(tenantId))
            return BadRequest("TenantId n�o encontrado no token.");
        var promotion = await _getPromotionByIdUseCase.ExecuteAsync(id, User);
        if (promotion == null || !promotion.IsActive)
            return BadRequest("Promo��o inv�lida ou inativa.");
        // Validar limites
        var totalUses = await _promotionRepository.GetUsageCountAsync(id, tenantId);
        var usesByCustomer = await _promotionRepository.GetUsageCountByCustomerAsync(id, tenantId, request.CustomerPhoneNumber);
        if (promotion.MaxTotalUses.HasValue && totalUses >= promotion.MaxTotalUses.Value)
            return BadRequest("Limite m�ximo de usos da promo��o atingido.");
        if (promotion.MaxUsesPerCustomer.HasValue && usesByCustomer >= promotion.MaxUsesPerCustomer.Value)
            return BadRequest("Limite m�ximo de usos da promo��o por cliente atingido.");
        // Registrar uso
        await _promotionRepository.AddUsageAsync(new PromotionUsage
        {
            CompanyId = tenantId,
            PromotionId = id,
            CustomerId = request.CustomerPhoneNumber, // Usando CustomerPhoneNumber como CustomerId temporariamente
            OrderId = request.OrderId,
            UsedAt = DateTime.UtcNow
        });
        return Ok();
    }
}
