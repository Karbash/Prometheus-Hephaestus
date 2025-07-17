using Hephaestus.Domain.DTOs.Request;
using Hephaestus.Domain.DTOs.Response;
using Hephaestus.Application.Interfaces.Coupon;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Security.Claims; // Necess�rio para ClaimTypes
using Hephaestus.Domain.Entities;
using Hephaestus.Domain.Repositories;
using Hephaestus.Application.Exceptions;

namespace Hephaestus.Controllers;

/// <summary>
/// Controller para gerenciamento de cupons, permitindo opera��es como cria��o, listagem, obten��o, atualiza��o e exclus�o de cupons.
/// </summary>
[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Admin,Tenant")]
public class CouponController : ControllerBase
{
    private readonly ICreateCouponUseCase _createCouponUseCase;
    private readonly IGetCouponsUseCase _getCouponsUseCase;
    private readonly IGetCouponByIdUseCase _getCouponByIdUseCase;
    private readonly IUpdateCouponUseCase _updateCouponUseCase;
    private readonly IDeleteCouponUseCase _deleteCouponUseCase;
    private readonly ILogger<CouponController> _logger;
    private readonly ICouponRepository _couponRepository;

    /// <summary>
    /// Inicializa uma nova inst�ncia do <see cref="CouponController"/>.
    /// </summary>
    /// <param name="createCouponUseCase">Caso de uso para criar cupons.</param>
    /// <param name="getCouponsUseCase">Caso de uso para listar cupons.</param>
    /// <param name="getCouponByIdUseCase">Caso de uso para obter um cupom por ID.</param>
    /// <param name="updateCouponUseCase">Caso de uso para atualizar um cupom.</param>
    /// <param name="deleteCouponUseCase">Caso de uso para deletar um cupom.</param>
    /// <param name="logger">Logger para registro de eventos e erros.</param>
    /// <param name="couponRepository">Reposit�rio para opera��es de cupom.</param>
    public CouponController(
        ICreateCouponUseCase createCouponUseCase,
        IGetCouponsUseCase getCouponsUseCase,
        IGetCouponByIdUseCase getCouponByIdUseCase,
        IUpdateCouponUseCase updateCouponUseCase,
        IDeleteCouponUseCase deleteCouponUseCase,
        ILogger<CouponController> logger,
        ICouponRepository couponRepository)
    {
        _createCouponUseCase = createCouponUseCase;
        _getCouponsUseCase = getCouponsUseCase;
        _getCouponByIdUseCase = getCouponByIdUseCase;
        _updateCouponUseCase = updateCouponUseCase;
        _deleteCouponUseCase = deleteCouponUseCase;
        _logger = logger;
        _couponRepository = couponRepository;
    }

    /// <summary>
    /// Cria um novo cupom para o tenant autenticado.
    /// </summary>
    /// <remarks>
    /// Este endpoint permite que um administrador ou um tenant crie um novo cupom de desconto.
    /// Requer autentica��o com as roles **Admin** ou **Tenant**.
    /// 
    /// Exemplo de requisi��o:
    /// ```json
    /// {
    ///   "code": "VERAO2025",
    ///   "description": "Desconto especial de ver�o",
    ///   "discountValue": 15.00,
    ///   "isActive": true,
    ///   "startDate": "2025-07-15T00:00:00Z",
    ///   "endDate": "2025-09-30T23:59:59Z"
    /// }
    /// ```
    /// 
    /// Exemplo de resposta de sucesso (Status 201 Created):
    /// ```json
    /// {
    ///   "id": "123e4567-e89b-12d3-a456-426614174001"
    /// }
    /// ```
    /// 
    /// Exemplo de erro de valida��o (Status 400 Bad Request):
    /// ```json
    /// {
    ///   "type": "[https://tools.ietf.org/html/rfc7231#section-6.5.1](https://tools.ietf.org/html/rfc7231#section-6.5.1)",
    ///   "title": "One or more validation errors occurred.",
    ///   "status": 400,
    ///   "errors": {
    ///     "Code": [
    ///       "O c�digo do cupom � obrigat�rio."
    ///     ]
    ///   }
    /// }
    /// ```
    /// Exemplo de erro de conflito (Status 409 Conflict):
    /// ```json
    /// {
    ///   "type": "[https://tools.ietf.org/html/rfc7231#section-6.5.8](https://tools.ietf.org/html/rfc7231#section-6.5.8)",
    ///   "title": "Conflict",
    ///   "status": 409,
    ///   "detail": "O c�digo 'VERAO2025' j� existe para este tenant."
    /// }
    /// ```
    /// </remarks>
    /// <param name="request">Dados do cupom a ser criado.</param>
    /// <returns>Um `CreatedAtActionResult` contendo o ID do cupom criado.</returns>
    [HttpPost]
    [SwaggerOperation(Summary = "Cria um novo cupom", Description = "Cria um cupom para o tenant autenticado. Requer autentica��o com Role=Admin ou Tenant.")]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(object))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ValidationProblemDetails))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status409Conflict, Type = typeof(ProblemDetails))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateCoupon([FromBody] CreateCouponRequest request)
    {
        var id = await _createCouponUseCase.ExecuteAsync(request, User);
        return CreatedAtAction(nameof(GetCouponById), new { id }, new { id });
    }

    /// <summary>
    /// Lista os cupons dispon�veis para o tenant autenticado, com filtros opcionais.
    /// </summary>
    /// <remarks>
    /// Este endpoint permite filtrar cupons por status de ativa��o (`isActive`) e pelo n�mero de telefone de um cliente associado (`customerPhoneNumber`).
    /// Requer autentica��o com as roles **Admin** ou **Tenant**.
    /// 
    /// Exemplo de resposta de sucesso (Status 200 OK):
    /// ```json
    /// [
    ///   {
    ///     "id": "123e4567-e89b-12d3-a456-426614174001",
    ///     "tenantId": "456e7890-e89b-12d3-a456-426614174002",
    ///     "code": "DESCONTO10",
    ///     "description": "10% de desconto",
    ///     "discountValue": 10.00,
    ///     "isActive": true,
    ///     "startDate": "2025-07-14T00:00:00Z",
    ///     "endDate": "2025-12-31T23:59:59Z"
    ///   },
    ///   {
    ///     "id": "a1b2c3d4-e5f6-7g8h-9i0j-k1l2m3n4o5p6",
    ///     "tenantId": "456e7890-e89b-12d3-a456-426614174002",
    ///     "code": "FRETEGRATIS",
    ///     "description": "Frete Gr�tis na primeira compra",
    ///     "discountValue": 0.00,
    ///     "isActive": false,
    ///     "startDate": "2025-06-01T00:00:00Z",
    ///     "endDate": "2025-06-30T23:59:59Z"
    ///   }
    /// ]
    /// ```
    /// Exemplo de erro de autentica��o (Status 401 Unauthorized):
    /// ```
    /// (Nenhum corpo de resposta, apenas status 401)
    /// ```
    /// </remarks>
    /// <param name="isActive">Filtro opcional para listar apenas cupons ativos ou inativos.</param>
    /// <param name="customerPhoneNumber">Filtro opcional para listar cupons associados a um n�mero de telefone de cliente espec�fico.</param>
    /// <returns>Uma `OkResult` contendo uma lista de `CouponResponse`.</returns>
    [HttpGet]
    [SwaggerOperation(Summary = "Lista cupons do tenant", Description = "Retorna a lista de cupons do tenant autenticado com filtros opcionais. Requer autentica��o com Role=Admin ou Tenant.")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<CouponResponse>))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetCoupons(
        [FromQuery] bool? isActive = null,
        [FromQuery] string? customerPhoneNumber = null,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? sortBy = null,
        [FromQuery] string? sortOrder = "asc")
    {
        var coupons = await _getCouponsUseCase.ExecuteAsync(User, isActive, customerPhoneNumber, pageNumber, pageSize, sortBy, sortOrder);
        return Ok(coupons);
    }

    /// <summary>
    /// Obt�m os detalhes de um cupom espec�fico por seu ID.
    /// </summary>
    /// <remarks>
    /// Este endpoint retorna todas as informa��es de um cupom, desde que perten�a ao tenant autenticado.
    /// Requer autentica��o com as roles **Admin** ou **Tenant**.
    /// 
    /// Exemplo de resposta de sucesso (Status 200 OK):
    /// ```json
    /// {
    ///   "id": "123e4567-e89b-12d3-a456-426614174001",
    ///   "tenantId": "456e7890-e89b-12d3-a456-426614174002",
    ///   "code": "DESCONTO10",
    ///   "description": "10% de desconto",
    ///   "discountValue": 10.00,
    ///   "isActive": true,
    ///   "startDate": "2025-07-14T00:00:00Z",
    ///   "endDate": "2025-12-31T23:59:59Z"
    /// }
    /// ```
    /// 
    /// Exemplo de erro (Status 400 Bad Request):
    /// ```json
    /// {
    ///   "type": "[https://tools.ietf.org/html/rfc7231#section-6.5.1](https://tools.ietf.org/html/rfc7231#section-6.5.1)",
    ///   "title": "Bad Request",
    ///   "status": 400,
    ///   "detail": "O ID do cupom 'invalido-id' n�o � um GUID v�lido."
    /// }
    /// ```
    /// Exemplo de erro (Status 404 Not Found):
    /// ```json
    /// {
    ///   "type": "[https://tools.ietf.org/html/rfc7231#section-6.5.4](https://tools.ietf.org/html/rfc7231#section-6.5.4)",
    ///   "title": "Not Found",
    ///   "status": 404,
    ///   "detail": "Cupom com ID '99999999-9999-9999-9999-999999999999' n�o encontrado para o tenant."
    /// }
    /// ```
    /// </remarks>
    /// <param name="id">O **ID (GUID)** do cupom a ser consultado.</param>
    /// <returns>Um `OkResult` contendo o `CouponResponse` ou um `NotFoundResult` se o cupom n�o for encontrado.</returns>
    [HttpGet("{id}")]
    [SwaggerOperation(Summary = "Obt�m cupom por ID", Description = "Retorna detalhes de um cupom do tenant. Requer autentica��o com Role=Admin ou Tenant.")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CouponResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ProblemDetails))] // Adicionado para ID inv�lido
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ProblemDetails))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetCouponById(string id)
    {
        var coupon = await _getCouponByIdUseCase.ExecuteAsync(id, User);
        return Ok(coupon);
    }

    /// <summary>
    /// Atualiza um cupom existente para o tenant autenticado.
    /// </summary>
    /// <remarks>
    /// Permite a modifica��o das propriedades de um cupom existente.
    /// Requer autentica��o com as roles **Admin** ou **Tenant**.
    /// 
    /// Exemplo de requisi��o:
    /// ```json
    /// {
    ///   "code": "SUPERDESCONTO20",
    ///   "description": "20% de desconto em toda a loja",
    ///   "discountValue": 20.00,
    ///   "isActive": true,
    ///   "startDate": "2025-07-14T00:00:00Z",
    ///   "endDate": "2026-01-31T23:59:59Z"
    /// }
    /// ```
    /// 
    /// Exemplo de resposta de sucesso (Status 204 No Content):
    /// ```
    /// (Nenhum corpo de resposta, apenas status 204)
    /// ```
    /// 
    /// Exemplo de erro de valida��o (Status 400 Bad Request):
    /// ```json
    /// {
    ///   "type": "[https://tools.ietf.org/html/rfc7231#section-6.5.1](https://tools.ietf.org/html/rfc7231#section-6.5.1)",
    ///   "title": "One or more validation errors occurred.",
    ///   "status": 400,
    ///   "errors": {
    ///     "DiscountValue": [
    ///       "O valor de desconto deve ser maior que zero."
    ///     ]
    ///   }
    /// }
    /// ```
    /// Exemplo de erro (Status 404 Not Found):
    /// ```json
    /// {
    ///   "type": "[https://tools.ietf.org/html/rfc7231#section-6.5.4](https://tools.ietf.org/html/rfc7231#section-6.5.4)",
    ///   "title": "Not Found",
    ///   "status": 404,
    ///   "detail": "Cupom com ID '99999999-9999-9999-9999-999999999999' n�o encontrado para atualiza��o."
    /// }
    /// ```
    /// Exemplo de erro de conflito (Status 409 Conflict):
    /// ```json
    /// {
    ///   "type": "[https://tools.ietf.org/html/rfc7231#section-6.5.8](https://tools.ietf.org/html/rfc7231#section-6.5.8)",
    ///   "title": "Conflict",
    ///   "status": 409,
    ///   "detail": "O c�digo de cupom 'SUPERDESCONTO20' j� est� em uso por outro cupom."
    /// }
    /// ```
    /// </remarks>
    /// <param name="id">O **ID (GUID)** do cupom a ser atualizado.</param>
    /// <param name="request">Dados atualizados do cupom.</param>
    /// <returns>Um `NoContentResult` em caso de sucesso na atualiza��o.</returns>
    [HttpPut("{id}")]
    [SwaggerOperation(Summary = "Atualiza um cupom", Description = "Atualiza um cupom do tenant autenticado. Requer autentica��o com Role=Admin ou Tenant.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ValidationProblemDetails))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ProblemDetails))]
    [ProducesResponseType(StatusCodes.Status409Conflict, Type = typeof(ProblemDetails))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateCoupon(string id, [FromBody] UpdateCouponRequest request)
    {
        await _updateCouponUseCase.ExecuteAsync(id, request, User);
        return NoContent();
    }

    /// <summary>
    /// Atualiza o status de ativa��o de um cupom (ativo/inativo).
    /// </summary>
    /// <param name="id">ID do cupom.</param>
    /// <param name="request">Novo status de ativa��o.</param>
    /// <returns>Cupom atualizado.</returns>
    /// <response code="200">Status atualizado com sucesso.</response>
    /// <response code="400">Regras de neg�cio violadas.</response>
    /// <response code="404">Cupom n�o encontrado.</response>
    [HttpPatch("{id}/status")]
    [SwaggerOperation(Summary = "Ativar/Desativar cupom", Description = "Ativa ou desativa um cupom, respeitando regras de neg�cio.")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CouponResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateCouponStatus(string id, [FromBody] UpdateCouponStatusRequest request)
    {
        var coupon = await _getCouponByIdUseCase.ExecuteAsync(id, User);
        if (coupon == null)
            return NotFound();

        // Regra: n�o pode ativar cupom expirado
        if (request.IsActive && coupon.EndDate < DateTime.UtcNow)
            return BadRequest("N�o � poss�vel ativar um cupom expirado.");

        // Regra: n�o pode desativar cupom em uso ativo (exemplo: cupom vinculado a pedido aberto)
        // Aqui seria necess�rio consultar pedidos, mas como exemplo:
        // bool emUso = await _orderRepository.ExistsOrderWithCouponActive(id);
        // if (!request.IsActive && emUso)
        //     return BadRequest("N�o � poss�vel desativar um cupom em uso ativo.");

        // Atualiza status
        var updateRequest = new UpdateCouponRequest
        {
            Code = coupon.Code,
            CustomerPhoneNumber = coupon.CustomerPhoneNumber,
            DiscountType = coupon.DiscountType,
            DiscountValue = coupon.DiscountValue,
            MenuItemId = coupon.MenuItemId,
            MinOrderValue = coupon.MinOrderValue,
            StartDate = coupon.StartDate,
            EndDate = coupon.EndDate,
            IsActive = request.IsActive
        };
        await _updateCouponUseCase.ExecuteAsync(id, updateRequest, User);
        // Retorna o cupom atualizado
        var updated = await _getCouponByIdUseCase.ExecuteAsync(id, User);
        return Ok(updated);
    }

    /// <summary>
    /// Registra o uso de um cupom por um cliente em um pedido.
    /// </summary>
    /// <param name="id">ID do cupom.</param>
    /// <param name="request">Dados do uso (cliente, pedido).</param>
    /// <returns>Confirma��o do uso.</returns>
    /// <response code="200">Uso registrado com sucesso.</response>
    /// <response code="400">Limite de uso atingido ou cupom inv�lido.</response>
    [HttpPost("{id}/use")]
    [SwaggerOperation(Summary = "Registrar uso de cupom", Description = "Registra o uso de um cupom por um cliente em um pedido, validando limites de uso.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UseCoupon(string id, [FromBody] UseCouponRequest request)
    {
        // Buscar cupom
        var tenantId = User.FindFirst("tenant_id")?.Value;
        if (string.IsNullOrEmpty(tenantId))
            return BadRequest("TenantId n�o encontrado no token.");
        var coupon = await _getCouponByIdUseCase.ExecuteAsync(id, User);
        if (coupon == null || !coupon.IsActive)
            return BadRequest("Cupom inv�lido ou inativo.");
        // Validar limites
        var totalUses = await _couponRepository.GetUsageCountAsync(id, tenantId);
        var usesByCustomer = await _couponRepository.GetUsageCountByCustomerAsync(id, tenantId, request.CustomerPhoneNumber);
        if (coupon.MaxTotalUses.HasValue && totalUses >= coupon.MaxTotalUses.Value)
            return BadRequest("Limite m�ximo de usos do cupom atingido.");
        if (coupon.MaxUsesPerCustomer.HasValue && usesByCustomer >= coupon.MaxUsesPerCustomer.Value)
            return BadRequest("Limite m�ximo de usos do cupom por cliente atingido.");
        // Registrar uso
        await _couponRepository.AddUsageAsync(new CouponUsage
        {
            TenantId = tenantId,
            CouponId = id,
            CustomerId = request.CustomerPhoneNumber, // Usando CustomerPhoneNumber como CustomerId temporariamente
            OrderId = request.OrderId,
            UsedAt = DateTime.UtcNow
        });
        return Ok();
    }

    /// <summary>
    /// Remove um cupom do tenant autenticado.
    /// </summary>
    /// <remarks>
    /// Este endpoint permite que um administrador ou um tenant remova um cupom existente.
    /// Requer autentica��o com as roles **Admin** ou **Tenant**.
    /// 
    /// Exemplo de resposta de sucesso (Status 204 No Content):
    /// ```
    /// (Nenhum corpo de resposta, apenas status 204)
    /// ```
    /// 
    /// Exemplo de erro (Status 400 Bad Request):
    /// ```json
    /// {
    ///   "type": "[https://tools.ietf.org/html/rfc7231#section-6.5.1](https://tools.ietf.org/html/rfc7231#section-6.5.1)",
    ///   "title": "Bad Request",
    ///   "status": 400,
    ///   "detail": "O ID do cupom 'invalido-id' n�o � um GUID v�lido."
    /// }
    /// ```
    /// 
    /// Exemplo de erro (Status 404 Not Found):
    /// ```json
    /// {
    ///   "type": "[https://tools.ietf.org/html/rfc7231#section-6.5.4](https://tools.ietf.org/html/rfc7231#section-6.5.4)",
    ///   "title": "Not Found",
    ///   "status": 404,
    ///   "detail": "Cupom com ID '99999999-9999-9999-9999-999999999999' n�o encontrado para o tenant."
    /// }
    /// ```
    /// 
    /// Exemplo de erro de conflito (Status 409 Conflict):
    /// ```json
    /// {
    ///   "type": "[https://tools.ietf.org/html/rfc7231#section-6.5.8](https://tools.ietf.org/html/rfc7231#section-6.5.8)",
    ///   "title": "Conflict",
    ///   "status": 409,
    ///   "detail": "N�o � poss�vel remover um cupom que est� sendo usado em pedidos ativos."
    /// }
    /// ```
    /// </remarks>
    /// <param name="id">O **ID (GUID)** do cupom a ser removido.</param>
    /// <returns>Um `NoContentResult` em caso de sucesso.</returns>
    [HttpDelete("{id}")]
    [SwaggerOperation(Summary = "Remove um cupom", Description = "Remove um cupom do tenant autenticado. Requer autentica��o com Role=Admin ou Tenant.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ProblemDetails))] // Adicionado para ID inv�lido
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ProblemDetails))]
    [ProducesResponseType(StatusCodes.Status409Conflict, Type = typeof(ProblemDetails))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteCoupon(string id)
    {
        await _deleteCouponUseCase.ExecuteAsync(id, User);
        return NoContent();
    }
}
