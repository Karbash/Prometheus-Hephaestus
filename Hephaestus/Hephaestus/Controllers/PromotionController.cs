using Hephaestus.Application.DTOs.Request;
using Hephaestus.Application.DTOs.Response;
using Hephaestus.Application.Interfaces.Promotion;
using Hephaestus.Domain.DTOs.Request;
using Hephaestus.Domain.DTOs.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Security.Claims; // Necessary for ClaimTypes

namespace Hephaestus.Controllers;

/// <summary>
/// Controller para gerenciamento completo de promoções de um tenant, incluindo criação, listagem,
/// obtenção por ID, atualização, exclusão e notificação de promoções.
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

    /// <summary>
    /// Inicializa uma nova instância do <see cref="PromotionController"/>.
    /// </summary>
    /// <param name="createPromotionUseCase">Caso de uso para criar promoções.</param>
    /// <param name="getPromotionsUseCase">Caso de uso para listar promoções.</param>
    /// <param name="getPromotionByIdUseCase">Caso de uso para obter promoção por ID.</param>
    /// <param name="updatePromotionUseCase">Caso de uso para atualizar promoções.</param>
    /// <param name="deletePromotionUseCase">Caso de uso para remover promoções.</param>
    /// <param name="notifyPromotionUseCase">Caso de uso para notificar promoções.</param>
    /// <param name="logger">Logger para registro de eventos e erros.</param>
    public PromotionController(
        ICreatePromotionUseCase createPromotionUseCase,
        IGetPromotionsUseCase getPromotionsUseCase,
        IGetPromotionByIdUseCase getPromotionByIdUseCase,
        IUpdatePromotionUseCase updatePromotionUseCase,
        IDeletePromotionUseCase deletePromotionUseCase,
        INotifyPromotionUseCase notifyPromotionUseCase,
        ILogger<PromotionController> logger)
    {
        _createPromotionUseCase = createPromotionUseCase;
        _getPromotionsUseCase = getPromotionsUseCase;
        _getPromotionByIdUseCase = getPromotionByIdUseCase;
        _updatePromotionUseCase = updatePromotionUseCase;
        _deletePromotionUseCase = deletePromotionUseCase;
        _notifyPromotionUseCase = notifyPromotionUseCase;
        _logger = logger;
    }

    /// CreatePromotion

    /// <summary>
    /// Cria uma nova promoção para o tenant autenticado.
    /// </summary>
    /// <remarks>
    /// Este endpoint permite que um tenant registre uma nova promoção em seu catálogo.
    /// Requer autenticação com a role **Tenant**.
    ///
    /// **Exemplo de Corpo da Requisição:**
    /// ```json
    /// {
    ///   "name": "Desconto de 10% no Verão",
    ///   "description": "10% de desconto em todos os pedidos acima de R$50 durante o verão.",
    ///   "discountType": "Percentage",
    ///   "discountValue": 10.00,
    ///   "menuItemId": null,
    ///   "minOrderValue": 50.00,
    ///   "maxUsagePerCustomer": 1,
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
    /// **Exemplo de Erro de Validação (Status 400 Bad Request):**
    /// ```json
    /// {
    ///   "type": "[https://tools.ietf.org/html/rfc7231#section-6.5.1](https://tools.ietf.org/html/rfc7231#section-6.5.1)",
    ///   "title": "One or more validation errors occurred.",
    ///   "status": 400,
    ///   "errors": {
    ///     "Name": [
    ///       "O campo 'Name' é obrigatório."
    ///     ],
    ///     "DiscountValue": [
    ///       "O valor do desconto deve ser positivo."
    ///     ]
    ///   }
    /// }
    /// ```
    ///
    /// **Exemplo de Erro de Autorização (Status 401 Unauthorized):**
    /// ```json
    /// {
    ///   "type": "[https://tools.ietf.org/html/rfc7235#section-3.1](https://tools.ietf.org/html/rfc7235#section-3.1)",
    ///   "title": "Unauthorized",
    ///   "status": 401,
    ///   "detail": "TenantId não encontrado no token."
    /// }
    /// ```
    ///
    /// **Exemplo de Erro Interno do Servidor (Status 500 Internal Server Error):**
    /// ```json
    /// {
    ///   "type": "[https://tools.ietf.org/html/rfc7231#section-6.6.1](https://tools.ietf.org/html/rfc7231#section-6.6.1)",
    ///   "title": "Internal Server Error",
    ///   "status": 500,
    ///   "detail": "Ocorreu um erro inesperado ao criar a promoção."
    /// }
    /// ```
    /// </remarks>
    /// <param name="request">Dados da promoção a ser criada (<see cref="CreatePromotionRequest"/>).</param>
    /// <returns>Um <see cref="CreatedAtActionResult"/> contendo o ID da promoção criada.</returns>
    [HttpPost]
    [SwaggerOperation(Summary = "Cria promoção", Description = "Cria uma nova promoção para o tenant. Requer autenticação com Role=Tenant.")]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(object))] // Retorna um objeto anônimo { id }
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ValidationProblemDetails))] // Detalhes do erro de validação
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ProblemDetails))] // Erro de autorização com detalhes
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ProblemDetails))] // Erro interno do servidor com detalhes
    public async Task<IActionResult> CreatePromotion([FromBody] CreatePromotionRequest request)
    {
        var id = await _createPromotionUseCase.ExecuteAsync(request, User);
        return CreatedAtAction(nameof(GetPromotionById), new { id }, new { id });
    }

    /// GetPromotions

    /// <summary>
    /// Lista as promoções do tenant autenticado.
    /// </summary>
    /// <remarks>
    /// Este endpoint retorna uma lista de promoções registradas para o tenant.
    /// É possível filtrar as promoções por status de ativação (`isActive`).
    /// Requer autenticação com a role **Tenant**.
    ///
    /// **Exemplo de Requisição:**
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
    ///     "name": "Desconto de 10% no Verão",
    ///     "description": "10% de desconto em todos os pedidos.",
    ///     "discountType": "Percentage",
    ///     "discountValue": 10.00,
    ///     "menuItemId": null,
    ///     "minOrderValue": 50.00,
    ///     "maxUsagePerCustomer": 1,
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
    ///     "name": "Frete Grátis para Novos Clientes",
    ///     "description": "Frete grátis na primeira compra.",
    ///     "discountType": "FreeShipping",
    ///     "discountValue": 0.00,
    ///     "menuItemId": null,
    ///     "minOrderValue": 30.00,
    ///     "maxUsagePerCustomer": 1,
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
    /// **Exemplo de Erro de Autorização (Status 401 Unauthorized):**
    /// ```json
    /// {
    ///   "type": "[https://tools.ietf.org/html/rfc7235#section-3.1](https://tools.ietf.org/html/rfc7235#section-3.1)",
    ///   "title": "Unauthorized",
    ///   "status": 401,
    ///   "detail": "TenantId não encontrado no token."
    /// }
    /// ```
    ///
    /// **Exemplo de Erro Interno do Servidor (Status 500 Internal Server Error):**
    /// ```json
    /// {
    ///   "type": "[https://tools.ietf.org/html/rfc7231#section-6.6.1](https://tools.ietf.org/html/rfc7231#section-6.6.1)",
    ///   "title": "Internal Server Error",
    ///   "status": 500,
    ///   "detail": "Ocorreu um erro inesperado ao listar as promoções."
    /// }
    /// ```
    /// </remarks>
    /// <param name="isActive">Filtro opcional: `true` para promoções ativas, `false` para inativas. Se omitido, retorna todas as promoções.</param>
    /// <returns>Um <see cref="OkObjectResult"/> contendo uma lista de objetos <see cref="PromotionResponse"/>.</returns>
    [HttpGet]
    [SwaggerOperation(Summary = "Lista promoções do tenant", Description = "Retorna uma lista paginada de promoções do tenant, com filtro opcional por status de ativação.")]
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
    /// Obtém os detalhes de uma promoção específica por ID.
    /// </summary>
    /// <remarks>
    /// Este endpoint retorna todas as informações de uma promoção, desde que a promoção pertença ao tenant autenticado.
    /// Requer autenticação com a role **Tenant**.
    ///
    /// **Exemplo de Requisição:**
    /// ```http
    /// GET /api/Promotion/123e4567-e89b-12d3-a456-426614174001
    /// ```
    ///
    /// **Exemplo de Resposta de Sucesso (Status 200 OK):**
    /// ```json
    /// {
    ///   "id": "123e4567-e89b-12d3-a456-426614174001",
    ///   "tenantId": "456e7890-e89b-12d3-a456-426614174002",
    ///   "name": "Desconto de 10% no Verão",
    ///   "description": "10% de desconto em todos os pedidos acima de R$50 durante o verão.",
    ///   "discountType": "Percentage",
    ///   "discountValue": 10.00,
    ///   "menuItemId": null,
    ///   "minOrderValue": 50.00,
    ///   "maxUsagePerCustomer": 1,
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
    ///   "detail": "O ID da promoção 'invalido-id' não é um GUID válido."
    /// }
    /// ```
    ///
    /// **Exemplo de Erro (Status 404 Not Found):**
    /// ```json
    /// {
    ///   "type": "[https://tools.ietf.org/html/rfc7231#section-6.5.4](https://tools.ietf.org/html/rfc7231#section-6.5.4)",
    ///   "title": "Not Found",
    ///   "status": 404,
    ///   "detail": "Promoção com ID '99999999-9999-9999-9999-999999999999' não encontrada para o tenant."
    /// }
    /// ```
    ///
    /// **Exemplo de Erro de Autorização (Status 401 Unauthorized):**
    /// ```json
    /// {
    ///   "type": "[https://tools.ietf.org/html/rfc7235#section-3.1](https://tools.ietf.org/html/rfc7235#section-3.1)",
    ///   "title": "Unauthorized",
    ///   "status": 401,
    ///   "detail": "TenantId não encontrado no token."
    /// }
    /// ```
    /// </remarks>
    /// <param name="id">O **ID (GUID)** da promoção a ser consultada.</param>
    /// <returns>Um <see cref="OkObjectResult"/> contendo o <see cref="PromotionResponse"/> ou um <see cref="NotFoundResult"/> se a promoção não for encontrada.</returns>
    [HttpGet("{id}")]
    [SwaggerOperation(Summary = "Obtém promoção por ID", Description = "Retorna detalhes de uma promoção do tenant. Requer autenticação com Role=Tenant.")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PromotionResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ProblemDetails))] // Para ID inválido
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
    /// Atualiza uma promoção existente para o tenant autenticado.
    /// </summary>
    /// <remarks>
    /// Este endpoint permite que um tenant atualize os dados de uma promoção existente.
    /// Requer autenticação com a role **Tenant**.
    ///
    /// **Exemplo de Corpo da Requisição:**
    /// ```json
    /// {
    ///   "id": "123e4567-e89b-12d3-a456-426614174001",
    ///   "name": "Desconto de 15% no Verão",
    ///   "description": "15% de desconto em todos os pedidos acima de R$60 durante o verão.",
    ///   "discountType": "Percentage",
    ///   "discountValue": 15.00,
    ///   "menuItemId": null,
    ///   "minOrderValue": 60.00,
    ///   "maxUsagePerCustomer": 2,
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
    /// **Exemplo de Erro de Validação (Status 400 Bad Request):**
    /// ```json
    /// {
    ///   "type": "[https://tools.ietf.org/html/rfc7231#section-6.5.1](https://tools.ietf.org/html/rfc7231#section-6.5.1)",
    ///   "title": "One or more validation errors occurred.",
    ///   "status": 400,
    ///   "errors": {
    ///     "Name": [
    ///       "O campo 'Name' é obrigatório."
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
    ///   "detail": "Promoção com ID '99999999-9999-9999-9999-999999999999' não encontrada para o tenant."
    /// }
    /// ```
    ///
    /// **Exemplo de Erro de Autorização (Status 401 Unauthorized):**
    /// ```json
    /// {
    ///   "type": "[https://tools.ietf.org/html/rfc7235#section-3.1](https://tools.ietf.org/html/rfc7235#section-3.1)",
    ///   "title": "Unauthorized",
    ///   "status": 401,
    ///   "detail": "TenantId não encontrado no token."
    /// }
    /// ```
    /// </remarks>
    /// <param name="id">O **ID (GUID)** da promoção a ser atualizada.</param>
    /// <param name="request">Dados atualizados da promoção (<see cref="UpdatePromotionRequest"/>).</param>
    /// <returns>Um <see cref="NoContentResult"/> em caso de sucesso.</returns>
    [HttpPut("{id}")]
    [SwaggerOperation(Summary = "Atualiza promoção", Description = "Atualiza uma promoção do tenant. Requer autenticação com Role=Tenant.")]
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
    /// Atualiza o status de ativação de uma promoção (ativa/inativa).
    /// </summary>
    /// <param name="id">ID da promoção.</param>
    /// <param name="request">Novo status de ativação.</param>
    /// <returns>Promoção atualizada.</returns>
    /// <response code="200">Status atualizado com sucesso.</response>
    /// <response code="400">Regras de negócio violadas.</response>
    /// <response code="404">Promoção não encontrada.</response>
    [HttpPatch("{id}/status")]
    [SwaggerOperation(Summary = "Ativar/Desativar promoção", Description = "Ativa ou desativa uma promoção, respeitando regras de negócio.")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PromotionResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdatePromotionStatus(string id, [FromBody] UpdatePromotionStatusRequest request)
    {
        var promotion = await _getPromotionByIdUseCase.ExecuteAsync(id, User);
        if (promotion == null)
            return NotFound();

        // Regra: não pode ativar promoção expirada
        if (request.IsActive && promotion.EndDate < DateTime.UtcNow)
            return BadRequest("Não é possível ativar uma promoção expirada.");

        // Regra: não pode desativar promoção em uso ativo (exemplo: promoção vinculada a pedido aberto)
        // Aqui seria necessário consultar pedidos, mas como exemplo:
        // bool emUso = await _orderRepository.ExistsOrderWithPromotionActive(id);
        // if (!request.IsActive && emUso)
        //     return BadRequest("Não é possível desativar uma promoção em uso ativo.");

        // Atualiza status
        var updateRequest = new UpdatePromotionRequest
        {
            Name = promotion.Name,
            Description = promotion.Description,
            DiscountType = promotion.DiscountType,
            DiscountValue = promotion.DiscountValue,
            MenuItemId = promotion.MenuItemId,
            MinOrderValue = promotion.MinOrderValue,
            MaxUsagePerCustomer = promotion.MaxUsagePerCustomer,
            MaxTotalUses = promotion.MaxTotalUses,
            ApplicableToTags = promotion.ApplicableToTags,
            StartDate = promotion.StartDate,
            EndDate = promotion.EndDate,
            IsActive = request.IsActive,
            ImageUrl = promotion.ImageUrl
        };
        await _updatePromotionUseCase.ExecuteAsync(id, updateRequest, User);
        // Retorna a promoção atualizada
        var updated = await _getPromotionByIdUseCase.ExecuteAsync(id, User);
        return Ok(updated);
    }

    /// DeletePromotion

    /// <summary>
    /// Remove uma promoção do tenant autenticado.
    /// </summary>
    /// <remarks>
    /// Este endpoint permite que um tenant remova uma promoção existente.
    /// Requer autenticação com a role **Tenant**.
    ///
    /// **Exemplo de Requisição:**
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
    ///   "detail": "O ID da promoção 'invalido-id' não é um GUID válido."
    /// }
    /// ```
    ///
    /// **Exemplo de Erro (Status 404 Not Found):**
    /// ```json
    /// {
    ///   "type": "[https://tools.ietf.org/html/rfc7231#section-6.5.4](https://tools.ietf.org/html/rfc7231#section-6.5.4)",
    ///   "title": "Not Found",
    ///   "status": 404,
    ///   "detail": "Promoção com ID '99999999-9999-9999-9999-999999999999' não encontrada para o tenant."
    /// }
    /// ```
    ///
    /// **Exemplo de Erro de Autorização (Status 401 Unauthorized):**
    /// ```json
    /// {
    ///   "type": "[https://tools.ietf.org/html/rfc7235#section-3.1](https://tools.ietf.org/html/rfc7235#section-3.1)",
    ///   "title": "Unauthorized",
    ///   "status": 401,
    ///   "detail": "TenantId não encontrado no token."
    /// }
    /// ```
    /// </remarks>
    /// <param name="id">O **ID (GUID)** da promoção a ser removida.</param>
    /// <returns>Um <see cref="NoContentResult"/> em caso de sucesso.</returns>
    [HttpDelete("{id}")]
    [SwaggerOperation(Summary = "Remove promoção", Description = "Remove uma promoção do tenant. Requer autenticação com Role=Tenant.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ProblemDetails))] // Para ID inválido
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
    /// Envia notificação de uma promoção via WhatsApp.
    /// </summary>
    /// <remarks>
    /// Este endpoint permite que um tenant envie notificações de promoções para seus clientes via WhatsApp.
    /// Requer autenticação com a role **Tenant**.
    ///
    /// **Exemplo de Corpo da Requisição:**
    /// ```json
    /// {
    ///   "promotionId": "123e4567-e89b-12d3-a456-426614174001",
    ///   "messageTemplate": "🎉 Promoção especial! {promotionName} - {promotionDescription} Válida até {endDate}. Aproveite!",
    ///   "customerPhoneNumbers": ["11987654321", "21987654321"]
    /// }
    /// ```
    ///
    /// **Exemplo de Resposta de Sucesso (Status 204 No Content):**
    /// ```
    /// (Nenhum corpo de resposta, apenas status 204)
    /// ```
    ///
    /// **Exemplo de Erro de Validação (Status 400 Bad Request):**
    /// ```json
    /// {
    ///   "type": "[https://tools.ietf.org/html/rfc7231#section-6.5.1](https://tools.ietf.org/html/rfc7231#section-6.5.1)",
    ///   "title": "One or more validation errors occurred.",
    ///   "status": 400,
    ///   "errors": {
    ///     "PromotionId": [
    ///       "O ID da promoção é obrigatório."
    ///     ],
    ///     "MessageTemplate": [
    ///       "O template da mensagem é obrigatório."
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
    ///   "detail": "Promoção com ID '99999999-9999-9999-9999-999999999999' não encontrada para o tenant."
    /// }
    /// ```
    ///
    /// **Exemplo de Erro de Autorização (Status 401 Unauthorized):**
    /// ```json
    /// {
    ///   "type": "[https://tools.ietf.org/html/rfc7235#section-3.1](https://tools.ietf.org/html/rfc7235#section-3.1)",
    ///   "title": "Unauthorized",
    ///   "status": 401,
    ///   "detail": "TenantId não encontrado no token."
    /// }
    /// ```
    /// </remarks>
    /// <param name="request">Dados da notificação (<see cref="NotifyPromotionRequest"/>).</param>
    /// <returns>Um <see cref="NoContentResult"/> em caso de sucesso.</returns>
    [HttpPost("notify")]
    [SwaggerOperation(Summary = "Notifica promoção via WhatsApp", Description = "Envia notificação de promoção via WhatsApp. Requer autenticação com Role=Tenant.")]
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
    /// Registra o uso de uma promoção por um cliente em um pedido.
    /// </summary>
    /// <param name="id">ID da promoção.</param>
    /// <param name="request">Dados do uso da promoção.</param>
    /// <returns>Confirmação do uso.</returns>
    /// <response code="200">Uso registrado com sucesso.</response>
    /// <response code="400">Regras de negócio violadas.</response>
    /// <response code="404">Promoção não encontrada.</response>
    [HttpPost("{id}/use")]
    [SwaggerOperation(Summary = "Registrar uso de promoção", Description = "Registra o uso de uma promoção por um cliente em um pedido, validando regras de negócio.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UsePromotion(string id, [FromBody] UsePromotionRequest request)
    {
        var promotion = await _getPromotionByIdUseCase.ExecuteAsync(id, User);
        if (promotion == null)
            return NotFound();

        if (!promotion.IsActive)
            return BadRequest("Promoção inativa.");
        if (promotion.EndDate < DateTime.UtcNow)
            return BadRequest("Promoção expirada.");
        // Exemplo de limite de uso (mock):
        // int maxUsos = promotion.MaxTotalUses ?? 1;
        // int usosCliente = 0; // Buscar na base real
        // if (usosCliente >= (promotion.MaxUsesPerCustomer ?? 1))
        //     return BadRequest("Limite de uso por cliente atingido.");
        // int usosTotais = 0; // Buscar na base real
        // if (usosTotais >= maxUsos)
        //     return BadRequest("Limite total de uso da promoção atingido.");

        // Aqui faria a atualização dos contadores de uso
        // await _promotionRepository.RegisterUseAsync(id, request.CustomerPhoneNumber, request.OrderId);

        return Ok(new { message = "Uso da promoção registrado com sucesso." });
    }
}