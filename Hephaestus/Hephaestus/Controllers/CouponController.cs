using Hephaestus.Application.DTOs.Request;
using Hephaestus.Application.DTOs.Response;
using Hephaestus.Application.Interfaces.Coupon;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Hephaestus.Controllers;

/// <summary>
/// Controller para gerenciamento de cupons.
/// </summary>
[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Tenant")]
public class CouponController : ControllerBase
{
    private readonly ICreateCouponUseCase _createCouponUseCase;
    private readonly IGetCouponsUseCase _getCouponsUseCase;
    private readonly IGetCouponByIdUseCase _getCouponByIdUseCase;
    private readonly IUpdateCouponUseCase _updateCouponUseCase;
    private readonly IDeleteCouponUseCase _deleteCouponUseCase;
    private readonly ILogger<CouponController> _logger;

    public CouponController(
        ICreateCouponUseCase createCouponUseCase,
        IGetCouponsUseCase getCouponsUseCase,
        IGetCouponByIdUseCase getCouponByIdUseCase,
        IUpdateCouponUseCase updateCouponUseCase,
        IDeleteCouponUseCase deleteCouponUseCase,
        ILogger<CouponController> logger)
    {
        _createCouponUseCase = createCouponUseCase;
        _getCouponsUseCase = getCouponsUseCase;
        _getCouponByIdUseCase = getCouponByIdUseCase;
        _updateCouponUseCase = updateCouponUseCase;
        _deleteCouponUseCase = deleteCouponUseCase;
        _logger = logger;
    }

    /// <summary>
    /// Cria um novo cupom.
    /// </summary>
    [HttpPost]
    [SwaggerOperation(Summary = "Cria cupom", Description = "Cria um novo cupom para o tenant. Requer autenticação com Role=Tenant.")]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(string))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(object))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(object))]
    [ProducesResponseType(StatusCodes.Status409Conflict, Type = typeof(object))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(object))]
    public async Task<IActionResult> CreateCoupon([FromBody] CreateCouponRequest request)
    {
        var tenantId = GetTenantId();
        var id = await _createCouponUseCase.ExecuteAsync(request, tenantId);
        return CreatedAtAction(nameof(GetCouponById), new { id }, new { id });
    }

    /// <summary>
    /// Lista cupons do tenant.
    /// </summary>
    [HttpGet]
    [SwaggerOperation(Summary = "Lista cupons", Description = "Retorna a lista de cupons do tenant. Requer autenticação com Role=Tenant.")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<CouponResponse>))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(object))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(object))]
    public async Task<IActionResult> GetCoupons([FromQuery] bool? isActive, [FromQuery] string? customerPhoneNumber)
    {
        var tenantId = GetTenantId();
        var coupons = await _getCouponsUseCase.ExecuteAsync(tenantId, isActive, customerPhoneNumber);
        return Ok(coupons);
    }

    /// <summary>
    /// Obtém detalhes de um cupom por ID.
    /// </summary>
    [HttpGet("{id}")]
    [SwaggerOperation(Summary = "Obtém cupom por ID", Description = "Retorna detalhes de um cupom. Requer autenticação com Role=Tenant.")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CouponResponse))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(object))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(object))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(object))]
    public async Task<IActionResult> GetCouponById(string id)
    {
        var tenantId = GetTenantId();
        var coupon = await _getCouponByIdUseCase.ExecuteAsync(id, tenantId);
        return Ok(coupon);
    }

    /// <summary>
    /// Atualiza um cupom.
    /// </summary>
    [HttpPut("{id}")]
    [SwaggerOperation(Summary = "Atualiza cupom", Description = "Atualiza um cupom do tenant. Requer autenticação com Role=Tenant.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(object))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(object))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(object))]
    [ProducesResponseType(StatusCodes.Status409Conflict, Type = typeof(object))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(object))]
    public async Task<IActionResult> UpdateCoupon(string id, [FromBody] UpdateCouponRequest request)
    {
        var tenantId = GetTenantId();
        await _updateCouponUseCase.ExecuteAsync(id, request, tenantId);
        return NoContent();
    }

    /// <summary>
    /// Remove um cupom.
    /// </summary>
    [HttpDelete("{id}")]
    [SwaggerOperation(Summary = "Remove cupom", Description = "Remove um cupom do tenant. Requer autenticação com Role=Tenant.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(object))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(object))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(object))]
    public async Task<IActionResult> DeleteCoupon(string id)
    {
        var tenantId = GetTenantId();
        await _deleteCouponUseCase.ExecuteAsync(id, tenantId);
        return NoContent();
    }

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