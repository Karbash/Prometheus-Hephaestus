using Hephaestus.Domain.DTOs.Request;
using Hephaestus.Application.DTOs.Response;
using Hephaestus.Application.Interfaces.Coupon;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Security.Claims; // Necessário para ClaimTypes

namespace Hephaestus.Controllers;

/// <summary>
/// Controller para gerenciamento de cupons, permitindo operações como criação, listagem, obtenção, atualização e exclusão de cupons.
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

    /// <summary>
    /// Inicializa uma nova instância do <see cref="CouponController"/>.
    /// </summary>
    /// <param name="createCouponUseCase">Caso de uso para criar cupons.</param>
    /// <param name="getCouponsUseCase">Caso de uso para listar cupons.</param>
    /// <param name="getCouponByIdUseCase">Caso de uso para obter um cupom por ID.</param>
    /// <param name="updateCouponUseCase">Caso de uso para atualizar um cupom.</param>
    /// <param name="deleteCouponUseCase">Caso de uso para deletar um cupom.</param>
    /// <param name="logger">Logger para registro de eventos e erros.</param>
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
    /// Cria um novo cupom para o tenant autenticado.
    /// </summary>
    /// <remarks>
    /// Este endpoint permite que um administrador ou um tenant crie um novo cupom de desconto.
    /// Requer autenticação com as roles **Admin** ou **Tenant**.
    /// 
    /// Exemplo de requisição:
    /// ```json
    /// {
    ///   "code": "VERAO2025",
    ///   "description": "Desconto especial de verão",
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
    /// Exemplo de erro de validação (Status 400 Bad Request):
    /// ```json
    /// {
    ///   "type": "[https://tools.ietf.org/html/rfc7231#section-6.5.1](https://tools.ietf.org/html/rfc7231#section-6.5.1)",
    ///   "title": "One or more validation errors occurred.",
    ///   "status": 400,
    ///   "errors": {
    ///     "Code": [
    ///       "O código do cupom é obrigatório."
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
    ///   "detail": "O código 'VERAO2025' já existe para este tenant."
    /// }
    /// ```
    /// </remarks>
    /// <param name="request">Dados do cupom a ser criado.</param>
    /// <returns>Um `CreatedAtActionResult` contendo o ID do cupom criado.</returns>
    [HttpPost]
    [SwaggerOperation(Summary = "Cria um novo cupom", Description = "Cria um cupom para o tenant autenticado. Requer autenticação com Role=Admin ou Tenant.")]
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
    /// Lista os cupons disponíveis para o tenant autenticado, com filtros opcionais.
    /// </summary>
    /// <remarks>
    /// Este endpoint permite filtrar cupons por status de ativação (`isActive`) e pelo número de telefone de um cliente associado (`customerPhoneNumber`).
    /// Requer autenticação com as roles **Admin** ou **Tenant**.
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
    ///     "description": "Frete Grátis na primeira compra",
    ///     "discountValue": 0.00,
    ///     "isActive": false,
    ///     "startDate": "2025-06-01T00:00:00Z",
    ///     "endDate": "2025-06-30T23:59:59Z"
    ///   }
    /// ]
    /// ```
    /// Exemplo de erro de autenticação (Status 401 Unauthorized):
    /// ```
    /// (Nenhum corpo de resposta, apenas status 401)
    /// ```
    /// </remarks>
    /// <param name="isActive">Filtro opcional para listar apenas cupons ativos ou inativos.</param>
    /// <param name="customerPhoneNumber">Filtro opcional para listar cupons associados a um número de telefone de cliente específico.</param>
    /// <returns>Uma `OkResult` contendo uma lista de `CouponResponse`.</returns>
    [HttpGet]
    [SwaggerOperation(Summary = "Lista cupons do tenant", Description = "Retorna a lista de cupons do tenant autenticado com filtros opcionais. Requer autenticação com Role=Admin ou Tenant.")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<CouponResponse>))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetCoupons([FromQuery] bool? isActive = null, [FromQuery] string? customerPhoneNumber = null)
    {
        var coupons = await _getCouponsUseCase.ExecuteAsync(User, isActive, customerPhoneNumber);
        return Ok(coupons);
    }

    /// <summary>
    /// Obtém os detalhes de um cupom específico por seu ID.
    /// </summary>
    /// <remarks>
    /// Este endpoint retorna todas as informações de um cupom, desde que pertença ao tenant autenticado.
    /// Requer autenticação com as roles **Admin** ou **Tenant**.
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
    ///   "detail": "O ID do cupom 'invalido-id' não é um GUID válido."
    /// }
    /// ```
    /// Exemplo de erro (Status 404 Not Found):
    /// ```json
    /// {
    ///   "type": "[https://tools.ietf.org/html/rfc7231#section-6.5.4](https://tools.ietf.org/html/rfc7231#section-6.5.4)",
    ///   "title": "Not Found",
    ///   "status": 404,
    ///   "detail": "Cupom com ID '99999999-9999-9999-9999-999999999999' não encontrado para o tenant."
    /// }
    /// ```
    /// </remarks>
    /// <param name="id">O **ID (GUID)** do cupom a ser consultado.</param>
    /// <returns>Um `OkResult` contendo o `CouponResponse` ou um `NotFoundResult` se o cupom não for encontrado.</returns>
    [HttpGet("{id}")]
    [SwaggerOperation(Summary = "Obtém cupom por ID", Description = "Retorna detalhes de um cupom do tenant. Requer autenticação com Role=Admin ou Tenant.")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CouponResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ProblemDetails))] // Adicionado para ID inválido
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
    /// Permite a modificação das propriedades de um cupom existente.
    /// Requer autenticação com as roles **Admin** ou **Tenant**.
    /// 
    /// Exemplo de requisição:
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
    /// Exemplo de erro de validação (Status 400 Bad Request):
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
    ///   "detail": "Cupom com ID '99999999-9999-9999-9999-999999999999' não encontrado para atualização."
    /// }
    /// ```
    /// Exemplo de erro de conflito (Status 409 Conflict):
    /// ```json
    /// {
    ///   "type": "[https://tools.ietf.org/html/rfc7231#section-6.5.8](https://tools.ietf.org/html/rfc7231#section-6.5.8)",
    ///   "title": "Conflict",
    ///   "status": 409,
    ///   "detail": "O código de cupom 'SUPERDESCONTO20' já está em uso por outro cupom."
    /// }
    /// ```
    /// </remarks>
    /// <param name="id">O **ID (GUID)** do cupom a ser atualizado.</param>
    /// <param name="request">Dados atualizados do cupom.</param>
    /// <returns>Um `NoContentResult` em caso de sucesso na atualização.</returns>
    [HttpPut("{id}")]
    [SwaggerOperation(Summary = "Atualiza um cupom", Description = "Atualiza um cupom do tenant autenticado. Requer autenticação com Role=Admin ou Tenant.")]
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
    /// Remove um cupom do tenant autenticado.
    /// </summary>
    /// <remarks>
    /// Este endpoint permite que um administrador ou um tenant remova um cupom existente.
    /// Requer autenticação com as roles **Admin** ou **Tenant**.
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
    ///   "detail": "O ID do cupom 'invalido-id' não é um GUID válido."
    /// }
    /// ```
    /// 
    /// Exemplo de erro (Status 404 Not Found):
    /// ```json
    /// {
    ///   "type": "[https://tools.ietf.org/html/rfc7231#section-6.5.4](https://tools.ietf.org/html/rfc7231#section-6.5.4)",
    ///   "title": "Not Found",
    ///   "status": 404,
    ///   "detail": "Cupom com ID '99999999-9999-9999-9999-999999999999' não encontrado para o tenant."
    /// }
    /// ```
    /// 
    /// Exemplo de erro de conflito (Status 409 Conflict):
    /// ```json
    /// {
    ///   "type": "[https://tools.ietf.org/html/rfc7231#section-6.5.8](https://tools.ietf.org/html/rfc7231#section-6.5.8)",
    ///   "title": "Conflict",
    ///   "status": 409,
    ///   "detail": "Não é possível remover um cupom que está sendo usado em pedidos ativos."
    /// }
    /// ```
    /// </remarks>
    /// <param name="id">O **ID (GUID)** do cupom a ser removido.</param>
    /// <returns>Um `NoContentResult` em caso de sucesso.</returns>
    [HttpDelete("{id}")]
    [SwaggerOperation(Summary = "Remove um cupom", Description = "Remove um cupom do tenant autenticado. Requer autenticação com Role=Admin ou Tenant.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ProblemDetails))] // Adicionado para ID inválido
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