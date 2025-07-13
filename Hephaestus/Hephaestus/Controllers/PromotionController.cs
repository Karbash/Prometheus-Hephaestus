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
    /// Exemplo de erro:
    /// ```json
    /// {
    ///   "errors": [
    ///     {
    ///       "propertyName": "Name",
    ///       "errorMessage": "Nome é obrigatório."
    ///     }
    ///   ]
    /// }
    /// ```
    /// </remarks>
    /// <param name="request">Dados da promoção a ser criada.</param>
    /// <returns>ID da promoção criada.</returns>
    /// <exception cref="ValidationException">Se os dados da requisição forem inválidos.</exception>
    /// <exception cref="InvalidOperationException">Se o item do cardápio não for encontrado para FreeItem.</exception>
    [HttpPost]
    [SwaggerOperation(Summary = "Cria promoção", Description = "Cria uma nova promoção para o tenant. Requer autenticação com Role=Tenant.")]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(object))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(object))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(object))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(object))]
    public async Task<IActionResult> CreatePromotion([FromBody] CreatePromotionRequest request)
    {
        try
        {
            var tenantId = User.FindFirst("TenantId")?.Value;
            if (string.IsNullOrEmpty(tenantId))
                return Unauthorized(new { error = "TenantId não encontrado no token." });

            var id = await _createPromotionUseCase.ExecuteAsync(request, tenantId);
            return CreatedAtAction(nameof(GetPromotionById), new { id }, new { id });
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning(ex, "Erro de validação ao criar promoção.");
            return BadRequest(new { errors = ex.Errors });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Erro ao criar promoção: {Message}.", ex.Message);
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar promoção.");
            return StatusCode(500, new { error = "Erro interno do servidor" });
        }
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
    /// Exemplo de erro:
    /// ```json
    /// {
    ///   "error": "TenantId não encontrado no token."
    /// }
    /// ```
    /// </remarks>
    /// <param name="isActive">Filtro opcional para promoções ativas (true) ou inativas (false).</param>
    /// <returns>Lista de promoções do tenant.</returns>
    /// <exception cref="Exception">Erro inesperado ao listar promoções.</exception>
    [HttpGet]
    [SwaggerOperation(Summary = "Lista promoções do tenant", Description = "Retorna a lista de promoções do tenant, com filtro opcional por status de ativação. Suporta cache Redis. Requer autenticação com Role=Tenant.")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<PromotionResponse>))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(object))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(object))]
    public async Task<IActionResult> GetPromotions([FromQuery] bool? isActive = null)
    {
        try
        {
            var tenantId = User.FindFirst("TenantId")?.Value;
            if (string.IsNullOrEmpty(tenantId))
                return Unauthorized(new { error = "TenantId não encontrado no token." });

            var promotions = await _getPromotionsUseCase.ExecuteAsync(tenantId, isActive);
            return Ok(promotions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao listar promoções.");
            return StatusCode(500, new { error = "Erro interno do servidor" });
        }
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
    /// <exception cref="KeyNotFoundException">Se a promoção não for encontrada.</exception>
    [HttpGet("{id}")]
    [SwaggerOperation(Summary = "Obtém promoção por ID", Description = "Retorna detalhes de uma promoção do tenant. Requer autenticação com Role=Tenant.")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PromotionResponse))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(object))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(object))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(object))]
    public async Task<IActionResult> GetPromotionById(string id)
    {
        try
        {
            var tenantId = User.FindFirst("TenantId")?.Value;
            if (string.IsNullOrEmpty(tenantId))
                return Unauthorized(new { error = "TenantId não encontrado no token." });

            var promotion = await _getPromotionByIdUseCase.ExecuteAsync(id, tenantId);
            return Ok(promotion);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Promoção {Id} não encontrada.", id);
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter promoção {Id}.", id);
            return StatusCode(500, new { error = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Atualiza uma promoção do tenant.
    /// </summary>
    /// <remarks>
    /// Exemplo de corpo da requisição:
    /// ```json
    /// {
    ///   "id": "123e4567-e89b-12d3-a456-426614174001",
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
    ///   "imageUrl": "https://exemplo.com/imagem-atualizada.jpg"
    /// }
    /// ```
    /// Exemplo de erro:
    /// ```json
    /// {
    ///   "error": "Promoção não encontrada."
    /// }
    /// ```
    /// </remarks>
    /// <param name="id">ID da promoção a ser atualizada (GUID).</param>
    /// <param name="request">Dados atualizados da promoção.</param>
    /// <returns>Status de sucesso (204 No Content).</returns>
    /// <exception cref="ValidationException">Se os dados da requisição forem inválidos.</exception>
    /// <exception cref="ArgumentException">Se o ID no corpo não corresponder ao ID na URL.</exception>
    /// <exception cref="KeyNotFoundException">Se a promoção não for encontrada.</exception>
    /// <exception cref="InvalidOperationException">Se o item do cardápio não for encontrado para FreeItem.</exception>
    [HttpPut("{id}")]
    [SwaggerOperation(Summary = "Atualiza promoção", Description = "Atualiza uma promoção do tenant. Requer autenticação com Role=Tenant.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(object))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(object))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(object))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(object))]
    public async Task<IActionResult> UpdatePromotion(string id, [FromBody] UpdatePromotionRequest request)
    {
        try
        {
            var tenantId = User.FindFirst("TenantId")?.Value;
            if (string.IsNullOrEmpty(tenantId))
                return Unauthorized(new { error = "TenantId não encontrado no token." });

            await _updatePromotionUseCase.ExecuteAsync(id, request, tenantId);
            return NoContent();
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning(ex, "Erro de validação ao atualizar promoção {Id}.", id);
            return BadRequest(new { errors = ex.Errors });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Erro ao atualizar promoção {Id}: {Message}.", id, ex.Message);
            return BadRequest(new { error = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Promoção {Id} não encontrada.", id);
            return NotFound(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Erro ao atualizar promoção {Id}: {Message}.", id, ex.Message);
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar promoção {Id}.", id);
            return StatusCode(500, new { error = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Remove uma promoção do tenant.
    /// </summary>
    /// <remarks>
    /// Exemplo de requisição:
    /// ```http
    /// DELETE /api/promotion/123e4567-e89b-12d3-a456-426614174001
    /// ```
    /// Exemplo de erro:
    /// ```json
    /// {
    ///   "error": "Promoção não encontrada."
    /// }
    /// ```
    /// </remarks>
    /// <param name="id">ID da promoção a ser removida (GUID).</param>
    /// <returns>Status de sucesso (204 No Content).</returns>
    /// <exception cref="KeyNotFoundException">Se a promoção não for encontrada.</exception>
    [HttpDelete("{id}")]
    [SwaggerOperation(Summary = "Remove promoção", Description = "Remove uma promoção do tenant. Requer autenticação com Role=Tenant.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(object))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(object))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(object))]
    public async Task<IActionResult> DeletePromotion(string id)
    {
        try
        {
            var tenantId = User.FindFirst("TenantId")?.Value;
            if (string.IsNullOrEmpty(tenantId))
                return Unauthorized(new { error = "TenantId não encontrado no token." });

            await _deletePromotionUseCase.ExecuteAsync(id, tenantId);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Promoção {Id} não encontrada.", id);
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao remover promoção {Id}.", id);
            return StatusCode(500, new { error = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Notifica uma promoção via WhatsApp.
    /// </summary>
    /// <remarks>
    /// Exemplo de corpo da requisição:
    /// ```json
    /// {
    ///   "promotionId": "123e4567-e89b-12d3-a456-426614174001",
    ///   "messageTemplate": "Confira nossa nova promoção: {name}!"
    /// }
    /// ```
    /// Exemplo de erro:
    /// ```json
    /// {
    ///   "errors": [
    ///     {
    ///       "propertyName": "PromotionId",
    ///       "errorMessage": "ID da promoção é obrigatório."
    ///     }
    ///   ]
    /// }
    /// ```
    /// </remarks>
    /// <param name="request">Dados da notificação (ID da promoção e modelo de mensagem).</param>
    /// <returns>Status de sucesso (204 No Content).</returns>
    /// <exception cref="ValidationException">Se os dados da requisição forem inválidos.</exception>
    /// <exception cref="KeyNotFoundException">Se a promoção não for encontrada.</exception>
    [HttpPost("notify")]
    [SwaggerOperation(Summary = "Notifica promoção via WhatsApp", Description = "Envia notificação de promoção via WhatsApp. Requer autenticação com Role=Tenant.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(object))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(object))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(object))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(object))]
    public async Task<IActionResult> NotifyPromotion([FromBody] NotifyPromotionRequest request)
    {
        try
        {
            var tenantId = User.FindFirst("TenantId")?.Value;
            if (string.IsNullOrEmpty(tenantId))
                return Unauthorized(new { error = "TenantId não encontrado no token." });

            await _notifyPromotionUseCase.ExecuteAsync(request, tenantId);
            return NoContent();
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning(ex, "Erro de validação ao notificar promoção.");
            return BadRequest(new { errors = ex.Errors });
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Promoção {PromotionId} não encontrada.", request.PromotionId);
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao notificar promoção {PromotionId}.", request.PromotionId);
            return StatusCode(500, new { error = "Erro interno do servidor" });
        }
    }
}