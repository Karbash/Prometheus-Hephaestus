using FluentValidation;
using Hephaestus.Application.DTOs.Request;
using Hephaestus.Application.DTOs.Response;
using Hephaestus.Application.Interfaces.Promotion;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Hephaestus.Controllers;

/// <summary>
/// Controller para gerenciamento de promoções de um tenant.
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
    /// <param name="logger">Logger para registro de erros.</param>
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

    /// <summary>
    /// Cria uma nova promoção para o tenant.
    /// </summary>
    /// <remarks>
    /// Exemplo de corpo da requisição:
    /// ```json
    /// {
    ///   "name": "Desconto de 10%",
    ///   "description": "10% de desconto em pedidos",
    ///   "discountType": "Percentage",
    ///   "discountValue": 10.00,
    ///   "menuItemId": null,
    ///   "minOrderValue": 50.00,
    ///   "maxUsagePerCustomer": 1,
    ///   "maxTotalUses": 100,
    ///   "applicableToTags": ["pizza", "lanche"],
    ///   "startDate": "2025-07-12T00:00:00",
    ///   "endDate": "2025-12-31T23:59:59",
    ///   "isActive": true,
    ///   "imageUrl": "https://exemplo.com/imagem.jpg"
    /// }
    /// ```
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
    /// <param name="request">Dados da promoção a ser criada.</param>
    /// <returns>ID da promoção criada.</returns>
    [HttpPost]
    [SwaggerOperation(Summary = "Cria promoção", Description = "Cria uma nova promoção para o tenant. Requer autenticação com Role=Tenant.")]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(object))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(object))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(object))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(object))]
    public async Task<IActionResult> CreatePromotion([FromBody] CreatePromotionRequest request)
    {
        var tenantId = GetTenantId();
        var id = await _createPromotionUseCase.ExecuteAsync(request, tenantId);
        return CreatedAtAction(nameof(GetPromotionById), new { id }, new { id });
    }

    /// <summary>
    /// Lista promoções do tenant.
    /// </summary>
    /// <remarks>
    /// Exemplo de requisição:
    /// ```http
    /// GET /api/promotion?isActive=true
    /// ```
    /// Exemplo de resposta de sucesso:
    /// ```json
    /// [
    ///   {
    ///     "id": "123e4567-e89b-12d3-a456-426614174001",
    ///     "tenantId": "456e7890-e89b-12d3-a456-426614174002",
    ///     "name": "Desconto de 10%",
    ///     "description": "10% de desconto em pedidos",
    ///     "discountType": "Percentage",
    ///     "discountValue": 10.00,
    ///     "menuItemId": null,
    ///     "minOrderValue": 50.00,
    ///     "maxUsagePerCustomer": 1,
    ///     "maxTotalUses": 100,
    ///     "applicableToTags": ["pizza", "lanche"],
    ///     "startDate": "2025-07-12T00:00:00",
    ///     "endDate": "2025-12-31T23:59:59",
    ///     "isActive": true,
    ///     "imageUrl": "https://exemplo.com/imagem.jpg"
    ///   }
    /// ]
    /// ```
    /// </remarks>
    /// <param name="isActive">Filtro opcional para promoções ativas (true) ou inativas (false).</param>
    /// <returns>Lista de promoções do tenant.</returns>
    [HttpGet]
    [SwaggerOperation(Summary = "Lista promoções do tenant", Description = "Retorna a lista de promoções do tenant, com filtro opcional por status de ativação. Suporta cache Redis. Requer autenticação com Role=Tenant.")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<PromotionResponse>))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(object))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(object))]
    public async Task<IActionResult> GetPromotions([FromQuery] bool? isActive = null)
    {
        var tenantId = GetTenantId();
        var promotions = await _getPromotionsUseCase.ExecuteAsync(tenantId, isActive);
        return Ok(promotions);
    }

    /// <summary>
    /// Obtém detalhes de uma promoção por ID.
    /// </summary>
    /// <remarks>
    /// Exemplo de requisição:
    /// ```http
    /// GET /api/promotion/123e4567-e89b-12d3-a456-426614174001
    /// ```
    /// Exemplo de resposta de sucesso:
    /// ```json
    /// {
    ///   "id": "123e4567-e89b-12d3-a456-426614174001",
    ///   "tenantId": "456e7890-e89b-12d3-a456-426614174002",
    ///   "name": "Desconto de 10%",
    ///   "description": "10% de desconto em pedidos",
    ///   "discountType": "Percentage",
    ///   "discountValue": 10.00,
    ///   "menuItemId": null,
    ///   "minOrderValue": 50.00,
    ///   "maxUsagePerCustomer": 1,
    ///   "maxTotalUses": 100,
    ///   "applicableToTags": ["pizza", "lanche"],
    ///   "startDate": "2025-07-12T00:00:00",
    ///   "endDate": "2025-12-31T23:59:59",
    ///   "isActive": true,
    ///   "imageUrl": "https://exemplo.com/imagem.jpg"
    /// }
    /// ```
    /// Exemplo de erro:
    /// ```json
    /// {
    ///   "error": "Promoção não encontrada."
    /// }
    /// ```
    /// </remarks>
    /// <param name="id">ID da promoção (GUID).</param>
    /// <returns>Detalhes da promoção.</returns>
    [HttpGet("{id}")]
    [SwaggerOperation(Summary = "Obtém promoção por ID", Description = "Retorna detalhes de uma promoção do tenant. Requer autenticação com Role=Tenant.")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PromotionResponse))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(object))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(object))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(object))]
    public async Task<IActionResult> GetPromotionById(string id)
    {
        var tenantId = GetTenantId();
        var promotion = await _getPromotionByIdUseCase.ExecuteAsync(id, tenantId);
        return Ok(promotion);
    }

    /// <summary>
    /// Atualiza uma promoção do tenant.
    /// </summary>
    /// <remarks>
    /// Exemplo de corpo da requisição:
    /// ```json
    /// {
    ///   "name": "Desconto de 15%",
    ///   "description": "15% de desconto em pedidos",
    ///   "discountType": "Percentage",
    ///   "discountValue": 15.00,
    ///   "menuItemId": null,
    ///   "minOrderValue": 60.00,
    ///   "maxUsagePerCustomer": 2,
    ///   "maxTotalUses": 200,
    ///   "applicableToTags": ["pizza", "lanche"],
    ///   "startDate": "2025-07-12T00:00:00",
    ///   "endDate": "2025-12-31T23:59:59",
    ///   "isActive": true,
    ///   "imageUrl": "https://exemplo.com/imagem.jpg"
    /// }
    /// ```
    /// Exemplo de resposta de sucesso:
    /// ```
    /// Status: 204 No Content
    /// ```
    /// Exemplo de erro:
    /// ```json
    /// {
    ///   "error": "Promoção não encontrada."
    /// }
    /// ```
    /// </remarks>
    /// <param name="id">ID da promoção a ser atualizada.</param>
    /// <param name="request">Dados atualizados da promoção.</param>
    /// <returns>Status da atualização.</returns>
    [HttpPut("{id}")]
    [SwaggerOperation(Summary = "Atualiza promoção", Description = "Atualiza uma promoção do tenant. Requer autenticação com Role=Tenant.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(object))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(object))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(object))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(object))]
    public async Task<IActionResult> UpdatePromotion(string id, [FromBody] UpdatePromotionRequest request)
    {
        var tenantId = GetTenantId();
        await _updatePromotionUseCase.ExecuteAsync(id, request, tenantId);
        return NoContent();
    }

    /// <summary>
    /// Remove uma promoção do tenant.
    /// </summary>
    /// <remarks>
    /// Exemplo de resposta de sucesso:
    /// ```
    /// Status: 204 No Content
    /// ```
    /// Exemplo de erro:
    /// ```json
    /// {
    ///   "error": "Promoção não encontrada."
    /// }
    /// ```
    /// </remarks>
    /// <param name="id">ID da promoção a ser removida.</param>
    /// <returns>Status da remoção.</returns>
    [HttpDelete("{id}")]
    [SwaggerOperation(Summary = "Remove promoção", Description = "Remove uma promoção do tenant. Requer autenticação com Role=Tenant.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(object))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(object))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(object))]
    public async Task<IActionResult> DeletePromotion(string id)
    {
        var tenantId = GetTenantId();
        await _deletePromotionUseCase.ExecuteAsync(id, tenantId);
        return NoContent();
    }

    /// <summary>
    /// Notifica uma promoção via WhatsApp.
    /// </summary>
    /// <remarks>
    /// Exemplo de corpo da requisição:
    /// ```json
    /// {
    ///   "promotionId": "123e4567-e89b-12d3-a456-426614174001",
    ///   "phoneNumbers": ["11987654321", "11987654322"],
    ///   "message": "Promoção especial para você!"
    /// }
    /// ```
    /// Exemplo de resposta de sucesso:
    /// ```
    /// Status: 204 No Content
    /// ```
    /// Exemplo de erro:
    /// ```json
    /// {
    ///   "error": "Promoção não encontrada."
    /// }
    /// ```
    /// </remarks>
    /// <param name="request">Dados da notificação.</param>
    /// <returns>Status da notificação.</returns>
    [HttpPost("notify")]
    [SwaggerOperation(Summary = "Notifica promoção via WhatsApp", Description = "Envia notificação de promoção via WhatsApp. Requer autenticação com Role=Tenant.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(object))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(object))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(object))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(object))]
    public async Task<IActionResult> NotifyPromotion([FromBody] NotifyPromotionRequest request)
    {
        var tenantId = GetTenantId();
        await _notifyPromotionUseCase.ExecuteAsync(request, tenantId);
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