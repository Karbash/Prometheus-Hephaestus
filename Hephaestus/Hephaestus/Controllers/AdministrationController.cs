using Hephaestus.Domain.DTOs.Response;
using Hephaestus.Domain.DTOs.Request;
using Hephaestus.Application.Interfaces.Administration;
using Hephaestus.Application.UseCases.Order;
using Hephaestus.Application.UseCases.Administration;
using Hephaestus.Application.Interfaces.Promotion;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Hephaestus.Controllers;

/// <summary>
/// Controller para gerenciamento administrativo de empresas, vendas e logs.
/// Todas as opera��es requerem autentica��o com a role "Admin" e a policy "RequireMfa".
/// </summary>
[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Admin", Policy = "RequireMfa")]
public class AdministrationController : ControllerBase
{
    private readonly IGetCompaniesUseCase _getCompaniesUseCase;
    private readonly IUpdateCompanyUseCase _updateCompanyUseCase;
    private readonly ISalesReportUseCase _salesReportUseCase;
    private readonly IAuditLogUseCase _auditLogUseCase;
    private readonly IGetCompaniesWithinRadiusUseCase _getCompaniesWithinRadiusUseCase;
    private readonly IGetAllPromotionsAdminUseCase _getAllPromotionsAdminUseCase;
    private readonly IGlobalMenuItemAdminUseCase _globalMenuItemAdminUseCase;
    private readonly IGlobalTagAdminUseCase _globalTagAdminUseCase;
    private readonly IGlobalCategoryAdminUseCase _globalCategoryAdminUseCase;
    private readonly IGlobalCustomerAdminUseCase _globalCustomerAdminUseCase;
    private readonly IGlobalCouponAdminUseCase _globalCouponAdminUseCase;
    private readonly IGlobalOrderAdminUseCase _globalOrderAdminUseCase;
    private readonly IGlobalOrderItemAdminUseCase _globalOrderItemAdminUseCase;
    private readonly IGlobalAddressAdminUseCase _globalAddressAdminUseCase;
    private readonly IGlobalAdditionalAdminUseCase _globalAdditionalAdminUseCase;
    private readonly IGlobalReviewAdminUseCase _globalReviewAdminUseCase;
    private readonly ILogger<AdministrationController> _logger;

    /// <summary>
    /// Inicializa uma nova inst�ncia do <see cref="AdministrationController"/>.
    /// </summary>
    /// <param name="getCompaniesUseCase">Caso de uso para listar empresas.</param>
    /// <param name="updateCompanyUseCase">Caso de uso para atualizar empresas.</param>
    /// <param name="salesReportUseCase">Caso de uso para relat�rios de vendas.</param>
    /// <param name="auditLogUseCase">Caso de uso para logs de auditoria.</param>
    /// <param name="getCompaniesWithinRadiusUseCase">Caso de uso para buscar empresas dentro de um raio.</param>
    /// <param name="logger">Logger para registro de erros.</param>
    public AdministrationController(
        IGetCompaniesUseCase getCompaniesUseCase,
        IUpdateCompanyUseCase updateCompanyUseCase,
        ISalesReportUseCase salesReportUseCase,
        IAuditLogUseCase auditLogUseCase,
        IGetCompaniesWithinRadiusUseCase getCompaniesWithinRadiusUseCase,
        IGetAllPromotionsAdminUseCase getAllPromotionsAdminUseCase,
        IGlobalMenuItemAdminUseCase globalMenuItemAdminUseCase,
        IGlobalTagAdminUseCase globalTagAdminUseCase,
        IGlobalCategoryAdminUseCase globalCategoryAdminUseCase,
        IGlobalCustomerAdminUseCase globalCustomerAdminUseCase,
        IGlobalCouponAdminUseCase globalCouponAdminUseCase,
        IGlobalOrderAdminUseCase globalOrderAdminUseCase,
        IGlobalOrderItemAdminUseCase globalOrderItemAdminUseCase,
        IGlobalAddressAdminUseCase globalAddressAdminUseCase,
        IGlobalAdditionalAdminUseCase globalAdditionalAdminUseCase,
        IGlobalReviewAdminUseCase globalReviewAdminUseCase,
        ILogger<AdministrationController> logger)
    {
        _getCompaniesUseCase = getCompaniesUseCase;
        _updateCompanyUseCase = updateCompanyUseCase;
        _salesReportUseCase = salesReportUseCase;
        _auditLogUseCase = auditLogUseCase;
        _getCompaniesWithinRadiusUseCase = getCompaniesWithinRadiusUseCase;
        _getAllPromotionsAdminUseCase = getAllPromotionsAdminUseCase;
        _globalMenuItemAdminUseCase = globalMenuItemAdminUseCase;
        _globalTagAdminUseCase = globalTagAdminUseCase;
        _globalCategoryAdminUseCase = globalCategoryAdminUseCase;
        _globalCustomerAdminUseCase = globalCustomerAdminUseCase;
        _globalCouponAdminUseCase = globalCouponAdminUseCase;
        _globalOrderAdminUseCase = globalOrderAdminUseCase;
        _globalOrderItemAdminUseCase = globalOrderItemAdminUseCase;
        _globalAddressAdminUseCase = globalAddressAdminUseCase;
        _globalAdditionalAdminUseCase = globalAdditionalAdminUseCase;
        _globalReviewAdminUseCase = globalReviewAdminUseCase;
        _logger = logger;
    }

    /// <summary>
    /// Lista todas as empresas registradas no sistema, com opes de filtro e paginao.
    /// </summary>
    /// <remarks>
    /// Requer **Role: Admin** e **MFA validado**.
    /// 
    /// Exemplo de resposta de sucesso (Status 200 OK):
    /// ```json
    /// {
    ///   "items": [
    ///     {
    ///       "id": "123e4567-e89b-12d3-a456-426614174001",
    ///       "name": "Empresa Exemplo",
    ///       "email": "exemplo@empresa.com",
    ///       "phoneNumber": "123456789",
    ///       "isEnabled": true,
    ///       "feeType": "Percentage",
    ///       "feeValue": 5.0,
    ///       "city": "S�o Paulo",
    ///       "neighborhood": "Vila Mariana",
    ///       "street": "Rua Exemplo",
    ///       "number": "123",
    ///       "latitude": -23.550520,
    ///       "longitude": -46.633308,
    ///       "slogan": "O melhor da cidade!",
    ///       "description": "Descri��o da empresa."
    ///     }
    ///   ],
    ///   "totalCount": 1,
    ///   "pageNumber": 1,
    ///   "pageSize": 20
    /// }
    /// ```
    /// </remarks>
    /// <param name="isEnabled">Filtro opcional para empresas habilitadas (true) ou desabilitadas (false).</param>
    /// <param name="pageNumber">N�mero da p�gina a ser retornada (padr�o: 1).</param>
    /// <param name="pageSize">N�mero de itens por p�gina (padr�o: 20).</param>
    /// <returns>Uma lista paginada de objetos `CompanyResponse`.</returns>
    [HttpGet("company")]
    [SwaggerOperation(Summary = "Lista todas as empresas", Description = "Retorna uma lista paginada de empresas, com filtro opcional por status de habilita��o (habilitadas ou desabilitadas).")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PagedResult<CompanyResponse>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ProblemDetails))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetCompanies(
        [FromQuery] bool? isEnabled = null,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20)
    {
        var companies = await _getCompaniesUseCase.ExecuteAsync(isEnabled, pageNumber, pageSize);
        return Ok(companies);
    }

    /// <summary>
    /// Atualiza as informa��es de uma empresa espec�fica.
    /// </summary>
    /// <remarks>
    /// Requer **Role: Admin** e **MFA validado**.
    /// O `id` na URL deve corresponder ao `Id` no corpo da requisi��o.
    /// 
    /// Exemplo de requisi��o:
    /// ```json
    /// {
    ///   "id": "123e4567-e89b-12d3-a456-426614174001",
    ///   "name": "Empresa Atualizada Ltda.",
    ///   "email": "contato_novo@empresa.com",
    ///   "phoneNumber": "998877665",
    ///   "apiKey": "nova_api_key_xyz",
    ///   "feeType": "Fixed",
    ///   "feeValue": 2.50,
    ///   "isEnabled": true,
    ///   "city": "Rio de Janeiro",
    ///   "neighborhood": "Botafogo",
    ///   "street": "Rua Exemplo Nova",
    ///   "number": "456",
    ///   "latitude": -22.951916,
    ///   "longitude": -43.210487,
    ///   "slogan": "Inova��o a cada passo!",
    ///   "description": "Empresa l�der em tecnologia."
    /// }
    /// ```
    /// 
    /// Exemplo de resposta de sucesso (Status 204 No Content):
    /// ```
    /// (Nenhum corpo de resposta)
    /// ```
    /// 
    /// Exemplo de erro (Status 404 Not Found):
    /// ```json
    /// {
    ///   "message": "Empresa com ID 'xyz' n�o encontrada."
    /// }
    /// ```
    /// </remarks>
    /// <param name="id">ID da empresa a ser atualizada (GUID).</param>
    /// <param name="request">Objeto com os dados atualizados da empresa.</param>
    /// <returns>Um `NoContentResult` em caso de sucesso.</returns>
    [HttpPut("company/{id}")]
    [SwaggerOperation(Summary = "Atualiza uma empresa", Description = "Atualiza as informa��es de uma empresa existente. O ID na URL deve coincidir com o da empresa a ser atualizada.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ProblemDetails))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ProblemDetails))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateCompany(string id, [FromBody] UpdateCompanyRequest request)
    {
        await _updateCompanyUseCase.ExecuteAsync(id, request, User);
        return NoContent();
    }

    /// <summary>
    /// Gera um relat�rio consolidado de vendas de todas as empresas, com filtros opcionais por data.
    /// </summary>
    /// <remarks>
    /// Requer **Role: Admin** e **MFA validado**.
    /// As datas devem estar no formato ISO 8601 (ex: `2024-01-01`).
    /// 
    /// Exemplo de resposta de sucesso (Status 200 OK):
    /// ```json
    /// {
    ///   "totalSales": 1000.00,
    ///   "totalTransactions": 50,
    ///   "averageTicket": 20.00,
    ///   "byCompany": [
    ///     {
    ///       "companyId": "123e4567-e89b-12d3-a456-426614174001",
    ///       "companyName": "Empresa Exemplo",
    ///       "totalSales": 500.00,
    ///       "totalTransactions": 25
    ///     },
    ///     {
    ///       "companyId": "a7b8c9d0-e1f2-3g4h-5i6j-7k8l9m0n1o2p",
    ///       "companyName": "Outra Empresa",
    ///       "totalSales": 500.00,
    ///       "totalTransactions": 25
    ///     }
    ///   ]
    /// }
    /// ```
    /// 
    /// Exemplo de erro (Status 400 Bad Request):
    /// ```json
    /// {
    ///   "message": "Data inicial n�o pode ser maior que a data final."
    /// }
    /// ```
    /// </remarks>
    /// <param name="startDate">Data inicial para filtrar as vendas (opcional, formato ISO 8601).</param>
    /// <param name="endDate">Data final para filtrar as vendas (opcional, formato ISO 8601).</param>
    /// <returns>Um objeto `SalesReportResponse` contendo o relat�rio de vendas.</returns>
    [HttpGet("sales/admin")]
    [SwaggerOperation(Summary = "Relat�rio de vendas consolidado", Description = "Retorna um relat�rio consolidado de vendas de todas as empresas, com filtros opcionais por per�odo.")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SalesReportResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ProblemDetails))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetSalesReport([FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
    {
        var report = await _salesReportUseCase.ExecuteAsync(startDate, endDate, User);
        return Ok(report);
    }

    /// <summary>
    /// Lista logs de auditoria de a��es administrativas.
    /// </summary>
    /// <remarks>
    /// Requer **Role: Admin** e **MFA validado**.
    /// As datas devem estar no formato ISO 8601 (ex: `2024-01-01`).
    /// 
    /// Exemplo de resposta de sucesso (Status 200 OK):
    /// ```json
    /// [
    ///   {
    ///     "id": "123e4567-e89b-12d3-a456-426614174001",
    ///     "userId": "456e7890-e89b-12d3-a456-426614174002",
    ///     "action": "UpdateCompany",
    ///     "entityId": "789e0123-e89b-12d3-a456-426614174003",
    ///     "description": "Empresa 'XYZ Corp' atualizada com sucesso",
    ///     "timestamp": "2024-07-14T15:30:00Z"
    ///   }
    /// ]
    /// ```
    /// </remarks>
    /// <param name="startDate">Data inicial para filtrar logs (opcional, formato ISO 8601).</param>
    /// <param name="endDate">Data final para filtrar logs (opcional, formato ISO 8601).</param>
    /// <returns>Uma lista de objetos `AuditLogResponse`.</returns>
    [HttpGet("audit-log")]
    [SwaggerOperation(Summary = "Lista logs de auditoria", Description = "Retorna logs de auditoria de a��es administrativas, com filtros opcionais por per�odo.")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<AuditLogResponse>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ProblemDetails))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAuditLogs([FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
    {
        var logs = await _auditLogUseCase.ExecuteAsync(startDate, endDate, User);
        return Ok(logs);
    }

    /// <summary>
    /// Lista todas as promoções do sistema, com filtros opcionais por status, empresa, paginação e ordenação.
    /// </summary>
    /// <remarks>
    /// Requer **Role: Admin** e **MFA validado**.
    /// </remarks>
    /// <param name="isActive">Filtro opcional para promoções ativas.</param>
    /// <param name="companyId">Filtro opcional por empresa.</param>
    /// <param name="pageNumber">Número da página (padrão: 1).</param>
    /// <param name="pageSize">Itens por página (padrão: 20).</param>
    /// <param name="sortBy">Campo de ordenação.</param>
    /// <param name="sortOrder">Ordem (asc/desc).</param>
    /// <returns>Uma lista paginada de promoções.</returns>
    [HttpGet("promotion")]
    [SwaggerOperation(Summary = "Lista todas as promoções", Description = "Retorna uma lista paginada de promoções, com filtros opcionais por status, empresa, paginação e ordenação.")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PagedResult<PromotionResponse>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ProblemDetails))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAllPromotions(
        [FromQuery] bool? isActive = null,
        [FromQuery] string? companyId = null,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? sortBy = null,
        [FromQuery] string? sortOrder = "asc")
    {
        var result = await _getAllPromotionsAdminUseCase.ExecuteAsync(isActive, companyId, pageNumber, pageSize, sortBy, sortOrder);
        return Ok(result);
    }

    /// <summary>
    /// Lista todos os itens do cardápio do sistema, com filtros avançados, paginação e ordenação.
    /// </summary>
    /// <remarks>
    /// Requer **Role: Admin** e **MFA validado**.
    /// </remarks>
    /// <param name="companyId">Filtro opcional por empresa.</param>
    /// <param name="tagIds">Filtro opcional por tags.</param>
    /// <param name="categoryIds">Filtro opcional por categorias.</param>
    /// <param name="maxPrice">Filtro opcional por preço máximo.</param>
    /// <param name="isAvailable">Filtro opcional por disponibilidade.</param>
    /// <param name="promotionActiveNow">Filtro por promoções ativas agora.</param>
    /// <param name="promotionDayOfWeek">Filtro por dia da semana da promoção.</param>
    /// <param name="promotionTime">Filtro por horário da promoção.</param>
    /// <param name="pageNumber">Número da página (padrão: 1).</param>
    /// <param name="pageSize">Itens por página (padrão: 20).</param>
    /// <param name="sortBy">Campo de ordenação.</param>
    /// <param name="sortOrder">Ordem (asc/desc).</param>
    /// <returns>Uma lista paginada de itens do cardápio.</returns>
    [HttpGet("menu-item")]
    [SwaggerOperation(Summary = "Lista todos os itens do cardápio", Description = "Retorna uma lista paginada de itens do cardápio, com filtros avançados, paginação e ordenação.")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PagedResult<MenuItemResponse>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ProblemDetails))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAllMenuItems(
        [FromQuery] string? companyId = null,
        [FromQuery] List<string>? tagIds = null,
        [FromQuery] List<string>? categoryIds = null,
        [FromQuery] decimal? maxPrice = null,
        [FromQuery] bool? isAvailable = null,
        [FromQuery] bool? promotionActiveNow = null,
        [FromQuery] int? promotionDayOfWeek = null,
        [FromQuery] string? promotionTime = null,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? sortBy = null,
        [FromQuery] string? sortOrder = "asc")
    {
        var result = await _globalMenuItemAdminUseCase.ExecuteAsync(companyId, tagIds, categoryIds, maxPrice, isAvailable, promotionActiveNow, promotionDayOfWeek, promotionTime, pageNumber, pageSize, sortBy, sortOrder);
        return Ok(result);
    }

    /// <summary>
    /// Lista todas as tags do sistema, com filtros por empresa, nome, paginação e ordenação.
    /// </summary>
    /// <remarks>
    /// Requer **Role: Admin** e **MFA validado**.
    /// </remarks>
    /// <param name="companyId">Filtro opcional por empresa.</param>
    /// <param name="name">Filtro opcional por nome da tag.</param>
    /// <param name="pageNumber">Número da página (padrão: 1).</param>
    /// <param name="pageSize">Itens por página (padrão: 20).</param>
    /// <param name="sortBy">Campo de ordenação.</param>
    /// <param name="sortOrder">Ordem (asc/desc).</param>
    /// <returns>Uma lista paginada de tags.</returns>
    [HttpGet("tag")]
    [SwaggerOperation(Summary = "Lista todas as tags", Description = "Retorna uma lista paginada de tags, com filtros por empresa, nome, paginação e ordenação.")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PagedResult<TagResponse>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ProblemDetails))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAllTags(
        [FromQuery] string? companyId = null,
        [FromQuery] string? name = null,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? sortBy = null,
        [FromQuery] string? sortOrder = "asc")
    {
        var result = await _globalTagAdminUseCase.ExecuteAsync(companyId, name, pageNumber, pageSize, sortBy, sortOrder);
        return Ok(result);
    }

    /// <summary>
    /// Lista todas as categorias do sistema, com filtros por empresa, nome, status, paginação e ordenação.
    /// </summary>
    /// <remarks>
    /// Requer **Role: Admin** e **MFA validado**.
    /// </remarks>
    /// <param name="companyId">Filtro opcional por empresa.</param>
    /// <param name="name">Filtro opcional por nome da categoria.</param>
    /// <param name="isActive">Filtro opcional por status de ativação.</param>
    /// <param name="pageNumber">Número da página (padrão: 1).</param>
    /// <param name="pageSize">Itens por página (padrão: 20).</param>
    /// <param name="sortBy">Campo de ordenação.</param>
    /// <param name="sortOrder">Ordem (asc/desc).</param>
    /// <returns>Uma lista paginada de categorias.</returns>
    [HttpGet("category")]
    [SwaggerOperation(Summary = "Lista todas as categorias", Description = "Retorna uma lista paginada de categorias, com filtros por empresa, nome, status, paginação e ordenação.")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PagedResult<CategoryResponse>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ProblemDetails))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAllCategories(
        [FromQuery] string? companyId = null,
        [FromQuery] string? name = null,
        [FromQuery] bool? isActive = null,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? sortBy = null,
        [FromQuery] string? sortOrder = "asc")
    {
        var result = await _globalCategoryAdminUseCase.ExecuteAsync(companyId, name, isActive, pageNumber, pageSize, sortBy, sortOrder);
        return Ok(result);
    }

    /// <summary>
    /// Lista todos os clientes do sistema, com filtros por empresa, nome, telefone, status, paginação e ordenação.
    /// </summary>
    /// <remarks>
    /// Requer **Role: Admin** e **MFA validado**.
    /// </remarks>
    /// <param name="companyId">Filtro opcional por empresa.</param>
    /// <param name="name">Filtro opcional por nome do cliente.</param>
    /// <param name="phoneNumber">Filtro opcional por telefone.</param>
    /// <param name="isActive">Filtro opcional por status de ativação.</param>
    /// <param name="pageNumber">Número da página (padrão: 1).</param>
    /// <param name="pageSize">Itens por página (padrão: 20).</param>
    /// <param name="sortBy">Campo de ordenação.</param>
    /// <param name="sortOrder">Ordem (asc/desc).</param>
    /// <returns>Uma lista paginada de clientes.</returns>
    [HttpGet("customer")]
    [SwaggerOperation(Summary = "Lista todos os clientes", Description = "Retorna uma lista paginada de clientes, com filtros por empresa, nome, telefone, status, paginação e ordenação.")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PagedResult<CustomerResponse>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ProblemDetails))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAllCustomers(
        [FromQuery] string? companyId = null,
        [FromQuery] string? name = null,
        [FromQuery] string? phoneNumber = null,
        [FromQuery] bool? isActive = null,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? sortBy = null,
        [FromQuery] string? sortOrder = "asc")
    {
        var result = await _globalCustomerAdminUseCase.ExecuteAsync(companyId, name, phoneNumber, isActive, pageNumber, pageSize, sortBy, sortOrder);
        return Ok(result);
    }

    /// <summary>
    /// Lista todos os cupons do sistema, com filtros por empresa, código, telefone do cliente, status, paginação e ordenação.
    /// </summary>
    /// <remarks>
    /// Requer **Role: Admin** e **MFA validado**.
    /// </remarks>
    /// <param name="companyId">Filtro opcional por empresa.</param>
    /// <param name="code">Filtro opcional por código do cupom.</param>
    /// <param name="customerPhoneNumber">Filtro opcional por telefone do cliente.</param>
    /// <param name="isActive">Filtro opcional por status de ativação.</param>
    /// <param name="pageNumber">Número da página (padrão: 1).</param>
    /// <param name="pageSize">Itens por página (padrão: 20).</param>
    /// <param name="sortBy">Campo de ordenação.</param>
    /// <param name="sortOrder">Ordem (asc/desc).</param>
    /// <returns>Uma lista paginada de cupons.</returns>
    [HttpGet("coupon")]
    [SwaggerOperation(Summary = "Lista todos os cupons", Description = "Retorna uma lista paginada de cupons, com filtros por empresa, código, telefone do cliente, status, paginação e ordenação.")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PagedResult<CouponResponse>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ProblemDetails))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAllCoupons(
        [FromQuery] string? companyId = null,
        [FromQuery] string? code = null,
        [FromQuery] string? customerPhoneNumber = null,
        [FromQuery] bool? isActive = null,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? sortBy = null,
        [FromQuery] string? sortOrder = "asc")
    {
        var result = await _globalCouponAdminUseCase.ExecuteAsync(companyId, code, customerPhoneNumber, isActive, pageNumber, pageSize, sortBy, sortOrder);
        return Ok(result);
    }

    /// <summary>
    /// Lista todos os pedidos do sistema, sem restrição de tenant, com filtros avançados, paginação e ordenação.
    /// </summary>
    /// <remarks>
    /// Requer <b>Role: Admin</b> e <b>MFA validado</b>.<br/>
    /// Permite filtrar por empresa, cliente, telefone, status, datas, valor, etc.
    /// </remarks>
    /// <param name="companyId">ID da empresa (opcional).</param>
    /// <param name="customerId">ID do cliente (opcional).</param>
    /// <param name="customerPhoneNumber">Telefone do cliente (opcional).</param>
    /// <param name="status">Status do pedido (opcional).</param>
    /// <param name="paymentStatus">Status do pagamento (opcional).</param>
    /// <param name="dataInicial">Data inicial de criação (opcional).</param>
    /// <param name="dataFinal">Data final de criação (opcional).</param>
    /// <param name="valorMin">Valor mínimo do pedido (opcional).</param>
    /// <param name="valorMax">Valor máximo do pedido (opcional).</param>
    /// <param name="pageNumber">Número da página (padrão: 1).</param>
    /// <param name="pageSize">Tamanho da página (padrão: 20).</param>
    /// <param name="sortBy">Campo de ordenação (opcional).</param>
    /// <param name="sortOrder">Ordem de ordenação (asc/desc, padrão: asc).</param>
    /// <returns>Lista paginada de pedidos.</returns>
    [HttpGet("order")]
    [SwaggerOperation(Summary = "Lista todos os pedidos (admin)", Description = "Retorna uma lista paginada de pedidos, com filtros avançados, sem restrição de tenant.")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PagedResult<OrderResponse>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ProblemDetails))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAllOrders(
        [FromQuery] string? companyId = null,
        [FromQuery] string? customerId = null,
        [FromQuery] string? customerPhoneNumber = null,
        [FromQuery] string? status = null,
        [FromQuery] string? paymentStatus = null,
        [FromQuery] DateTime? dataInicial = null,
        [FromQuery] DateTime? dataFinal = null,
        [FromQuery] decimal? valorMin = null,
        [FromQuery] decimal? valorMax = null,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? sortBy = null,
        [FromQuery] string? sortOrder = "asc")
    {
        var result = await _globalOrderAdminUseCase.ExecuteAsync(
            companyId,
            customerId,
            customerPhoneNumber,
            status,
            paymentStatus,
            dataInicial,
            dataFinal,
            valorMin,
            valorMax,
            pageNumber,
            pageSize,
            sortBy,
            sortOrder);
        return Ok(result);
    }

    /// <summary>
    /// Lista todos os itens de pedido do sistema, sem restrição de tenant, com filtros avançados, paginação e ordenação.
    /// </summary>
    /// <remarks>
    /// Requer <b>Role: Admin</b> e <b>MFA validado</b>.<br/>
    /// Permite filtrar por pedido, empresa, cliente, produto, datas, valor, etc.
    /// </remarks>
    /// <param name="orderId">ID do pedido (opcional).</param>
    /// <param name="companyId">ID da empresa (opcional).</param>
    /// <param name="customerId">ID do cliente (opcional).</param>
    /// <param name="menuItemId">ID do item do cardápio (opcional).</param>
    /// <param name="dataInicial">Data inicial de criação (opcional).</param>
    /// <param name="dataFinal">Data final de criação (opcional).</param>
    /// <param name="valorMin">Valor unitário mínimo (opcional).</param>
    /// <param name="valorMax">Valor unitário máximo (opcional).</param>
    /// <param name="pageNumber">Número da página (padrão: 1).</param>
    /// <param name="pageSize">Tamanho da página (padrão: 20).</param>
    /// <param name="sortBy">Campo de ordenação (opcional).</param>
    /// <param name="sortOrder">Ordem de ordenação (asc/desc, padrão: asc).</param>
    /// <returns>Lista paginada de itens de pedido.</returns>
    [HttpGet("order-item")]
    [SwaggerOperation(Summary = "Lista todos os itens de pedido (admin)", Description = "Retorna uma lista paginada de itens de pedido, com filtros avançados, sem restrição de tenant.")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PagedResult<OrderItemResponse>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ProblemDetails))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAllOrderItems(
        [FromQuery] string? orderId = null,
        [FromQuery] string? companyId = null,
        [FromQuery] string? customerId = null,
        [FromQuery] string? menuItemId = null,
        [FromQuery] DateTime? dataInicial = null,
        [FromQuery] DateTime? dataFinal = null,
        [FromQuery] decimal? valorMin = null,
        [FromQuery] decimal? valorMax = null,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? sortBy = null,
        [FromQuery] string? sortOrder = "asc")
    {
        var result = await _globalOrderItemAdminUseCase.ExecuteAsync(
            orderId,
            companyId,
            customerId,
            menuItemId,
            dataInicial,
            dataFinal,
            valorMin,
            valorMax,
            pageNumber,
            pageSize,
            sortBy,
            sortOrder);
        return Ok(result);
    }

    /// <summary>
    /// Lista todos os endereços do sistema, sem restrição de tenant, com filtros avançados, paginação e ordenação.
    /// </summary>
    /// <remarks>
    /// Requer <b>Role: Admin</b> e <b>MFA validado</b>.<br/>
    /// Permite filtrar por entidade, tipo, cidade, estado, datas, etc.
    /// </remarks>
    /// <param name="entityId">ID da entidade (empresa ou cliente, opcional).</param>
    /// <param name="entityType">Tipo da entidade ("Company", "Customer", etc., opcional).</param>
    /// <param name="city">Cidade (opcional).</param>
    /// <param name="state">Estado (opcional).</param>
    /// <param name="type">Tipo do endereço (Principal, Entrega, etc., opcional).</param>
    /// <param name="dataInicial">Data inicial de criação (opcional).</param>
    /// <param name="dataFinal">Data final de criação (opcional).</param>
    /// <param name="pageNumber">Número da página (padrão: 1).</param>
    /// <param name="pageSize">Tamanho da página (padrão: 20).</param>
    /// <param name="sortBy">Campo de ordenação (opcional).</param>
    /// <param name="sortOrder">Ordem de ordenação (asc/desc, padrão: asc).</param>
    /// <returns>Lista paginada de endereços.</returns>
    [HttpGet("address")]
    [SwaggerOperation(Summary = "Lista todos os endereços (admin)", Description = "Retorna uma lista paginada de endereços, com filtros avançados, sem restrição de tenant.")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PagedResult<AddressResponse>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ProblemDetails))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAllAddresses(
        [FromQuery] string? entityId = null,
        [FromQuery] string? entityType = null,
        [FromQuery] string? city = null,
        [FromQuery] string? state = null,
        [FromQuery] string? type = null,
        [FromQuery] DateTime? dataInicial = null,
        [FromQuery] DateTime? dataFinal = null,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? sortBy = null,
        [FromQuery] string? sortOrder = "asc")
    {
        var result = await _globalAddressAdminUseCase.ExecuteAsync(
            entityId,
            entityType,
            city,
            state,
            type,
            dataInicial,
            dataFinal,
            pageNumber,
            pageSize,
            sortBy,
            sortOrder);
        return Ok(result);
    }

    /// <summary>
    /// Lista todos os adicionais do sistema, sem restrição de tenant, com filtros avançados, paginação e ordenação.
    /// </summary>
    /// <remarks>
    /// Requer <b>Role: Admin</b> e <b>MFA validado</b>.<br/>
    /// Permite filtrar por empresa, nome, disponibilidade, preço, datas, etc.
    /// </remarks>
    /// <param name="tenantId">ID da empresa (opcional).</param>
    /// <param name="name">Nome do adicional (opcional).</param>
    /// <param name="isAvailable">Disponibilidade (opcional).</param>
    /// <param name="precoMin">Preço mínimo (opcional).</param>
    /// <param name="precoMax">Preço máximo (opcional).</param>
    /// <param name="dataInicial">Data inicial de criação (opcional).</param>
    /// <param name="dataFinal">Data final de criação (opcional).</param>
    /// <param name="pageNumber">Número da página (padrão: 1).</param>
    /// <param name="pageSize">Tamanho da página (padrão: 20).</param>
    /// <param name="sortBy">Campo de ordenação (opcional).</param>
    /// <param name="sortOrder">Ordem de ordenação (asc/desc, padrão: asc).</param>
    /// <returns>Lista paginada de adicionais.</returns>
    [HttpGet("additional")]
    [SwaggerOperation(Summary = "Lista todos os adicionais (admin)", Description = "Retorna uma lista paginada de adicionais, com filtros avançados, sem restrição de tenant.")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PagedResult<AdditionalResponse>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ProblemDetails))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAllAdditionals(
        [FromQuery] string? tenantId = null,
        [FromQuery] string? name = null,
        [FromQuery] bool? isAvailable = null,
        [FromQuery] decimal? precoMin = null,
        [FromQuery] decimal? precoMax = null,
        [FromQuery] DateTime? dataInicial = null,
        [FromQuery] DateTime? dataFinal = null,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? sortBy = null,
        [FromQuery] string? sortOrder = "asc")
    {
        var result = await _globalAdditionalAdminUseCase.ExecuteAsync(
            tenantId,
            name,
            isAvailable,
            precoMin,
            precoMax,
            dataInicial,
            dataFinal,
            pageNumber,
            pageSize,
            sortBy,
            sortOrder);
        return Ok(result);
    }

    /// <summary>
    /// Lista todas as avaliações do sistema, sem restrição de tenant, com filtros avançados, paginação e ordenação.
    /// </summary>
    /// <remarks>
    /// Requer <b>Role: Admin</b> e <b>MFA validado</b>.<br/>
    /// Permite filtrar por empresa, cliente, pedido, nota, status, datas, etc.
    /// </remarks>
    /// <param name="companyId">ID da empresa (opcional).</param>
    /// <param name="customerId">ID do cliente (opcional).</param>
    /// <param name="orderId">ID do pedido (opcional).</param>
    /// <param name="ratingMin">Nota mínima (opcional).</param>
    /// <param name="ratingMax">Nota máxima (opcional).</param>
    /// <param name="isActive">Status de ativação (opcional).</param>
    /// <param name="dataInicial">Data inicial de criação (opcional).</param>
    /// <param name="dataFinal">Data final de criação (opcional).</param>
    /// <param name="pageNumber">Número da página (padrão: 1).</param>
    /// <param name="pageSize">Tamanho da página (padrão: 20).</param>
    /// <param name="sortBy">Campo de ordenação (opcional).</param>
    /// <param name="sortOrder">Ordem de ordenação (asc/desc, padrão: asc).</param>
    /// <returns>Lista paginada de avaliações.</returns>
    [HttpGet("review")]
    [SwaggerOperation(Summary = "Lista todas as avaliações (admin)", Description = "Retorna uma lista paginada de avaliações, com filtros avançados, sem restrição de tenant.")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PagedResult<ReviewResponse>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ProblemDetails))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAllReviews(
        [FromQuery] string? companyId = null,
        [FromQuery] string? customerId = null,
        [FromQuery] string? orderId = null,
        [FromQuery] int? ratingMin = null,
        [FromQuery] int? ratingMax = null,
        [FromQuery] bool? isActive = null,
        [FromQuery] DateTime? dataInicial = null,
        [FromQuery] DateTime? dataFinal = null,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? sortBy = null,
        [FromQuery] string? sortOrder = "asc")
    {
        var result = await _globalReviewAdminUseCase.ExecuteAsync(
            companyId,
            customerId,
            orderId,
            ratingMin,
            ratingMax,
            isActive,
            dataInicial,
            dataFinal,
            pageNumber,
            pageSize,
            sortBy,
            sortOrder);
        return Ok(result);
    }

    /// <summary>
    /// Lista empresas localizadas dentro de um raio geográfico específico, com filtros opcionais por tipo de comida (tags) e categoria.
    /// </summary>
    /// <remarks>
    /// Requer **Role: Admin** e **MFA validado**.
    ///
    /// Exemplo de requisição com filtros:
    /// GET /api/administration/company/radius?centerLat=-23.5&centerLon=-46.6&radiusKm=2&tagIds=tag1&tagIds=tag2&categoryIds=cat1
    /// </remarks>
    /// <param name="centerLat">Latitude do centro do raio.</param>
    /// <param name="centerLon">Longitude do centro do raio.</param>
    /// <param name="radiusKm">Raio em quilômetros para a busca.</param>
    /// <param name="city">Filtro opcional por cidade.</param>
    /// <param name="neighborhood">Filtro opcional por bairro.</param>
    /// <param name="tagIds">Lista opcional de IDs de tags (tipo de comida).</param>
    /// <param name="categoryIds">Lista opcional de IDs de categorias.</param>
    /// <param name="maxPrice">Preço máximo para filtrar empresas que tenham pelo menos um item do cardápio até esse valor (opcional).</param>
    /// <param name="openNow">Se verdadeiro, retorna apenas empresas abertas no momento da consulta (opcional).</param>
    /// <param name="dayOfWeek">Dia da semana para filtrar empresas abertas (0=domingo, 1=segunda, ... 6=sábado, opcional).</param>
    /// <param name="time">Horário (HH:mm) para filtrar empresas abertas em um horário específico (opcional).</param>
    /// <param name="promotionActiveNow">Se verdadeiro, retorna apenas empresas com promoções ativas no momento (opcional).</param>
    /// <param name="promotionDayOfWeek">Dia da semana para filtrar promoções ativas (0=domingo, ..., 6=sábado, opcional).</param>
    /// <param name="promotionTime">Horário (HH:mm) para filtrar promoções ativas em um horário específico (opcional).</param>
    /// <returns>Uma lista de objetos `CompanyResponse` que estão dentro do raio especificado e atendem aos filtros.</returns>
    [HttpGet("company/radius")]
    [SwaggerOperation(Summary = "Lista empresas por raio geográfico", Description = "Retorna empresas localizadas dentro de um raio especificado (em km) a partir de uma coordenada central, com filtros opcionais por cidade, bairro, tipo de comida (tags), categoria, preço máximo, horário de funcionamento e promoções ativas.")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<CompanyResponse>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ProblemDetails))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetCompaniesByRadius(
        [FromQuery] double centerLat,
        [FromQuery] double centerLon,
        [FromQuery] double radiusKm,
        [FromQuery] string? city = null,
        [FromQuery] string? neighborhood = null,
        [FromQuery] List<string>? tagIds = null,
        [FromQuery] List<string>? categoryIds = null,
        [FromQuery] decimal? maxPrice = null,
        [FromQuery] bool? openNow = null,
        [FromQuery] int? dayOfWeek = null,
        [FromQuery] string? time = null,
        [FromQuery] bool? promotionActiveNow = null,
        [FromQuery] int? promotionDayOfWeek = null,
        [FromQuery] string? promotionTime = null)
    {
        var companies = await _getCompaniesWithinRadiusUseCase.ExecuteAsync(centerLat, centerLon, radiusKm, city, neighborhood, tagIds, categoryIds, maxPrice, openNow, dayOfWeek, time, promotionActiveNow, promotionDayOfWeek, promotionTime);
        return Ok(companies);
    }
}
