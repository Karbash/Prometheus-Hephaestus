using Hephaestus.Domain.DTOs.Response;
using Hephaestus.Domain.DTOs.Request;
using Hephaestus.Application.Interfaces.Administration;
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
        ILogger<AdministrationController> logger)
    {
        _getCompaniesUseCase = getCompaniesUseCase;
        _updateCompanyUseCase = updateCompanyUseCase;
        _salesReportUseCase = salesReportUseCase;
        _auditLogUseCase = auditLogUseCase;
        _getCompaniesWithinRadiusUseCase = getCompaniesWithinRadiusUseCase;
        _logger = logger;
    }

    /// <summary>
    /// Lista todas as empresas registradas no sistema, com op��es de filtro e pagina��o.
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
    /// Lista empresas localizadas dentro de um raio geogr�fico espec�fico.
    /// </summary>
    /// <remarks>
    /// Requer **Role: Admin** e **MFA validado**.
    /// 
    /// Exemplo de resposta de sucesso (Status 200 OK):
    /// ```json
    /// [
    ///   {
    ///     "id": "123e4567-e89b-12d3-a456-426614174001",
    ///     "name": "Empresa Pr�xima",
    ///     "email": "proxima@empresa.com",
    ///     "phoneNumber": "123456789",
    ///     "isEnabled": true,
    ///     "feeType": "Percentage",
    ///     "feeValue": 5.0,
    ///     "city": "S�o Paulo",
    ///     "neighborhood": "Vila Mariana",
    ///     "street": "Rua Exemplo",
    ///     "number": "123",
    ///     "latitude": -23.550520,
    ///     "longitude": -46.633308,
    ///     "slogan": "O melhor da cidade!",
    ///     "description": "Descri��o da empresa."
    ///   }
    /// ]
    /// ```
    /// 
    /// Exemplo de erro (Status 400 Bad Request):
    /// ```json
    /// {
    ///   "message": "Par�metros de localiza��o inv�lidos. Latitude e Longitude devem ser fornecidos."
    /// }
    /// ```
    /// </remarks>
    /// <param name="centerLat">Latitude do centro do raio.</param>
    /// <param name="centerLon">Longitude do centro do raio.</param>
    /// <param name="radiusKm">Raio em quil�metros para a busca.</param>
    /// <param name="city">Filtro opcional por cidade.</param>
    /// <param name="neighborhood">Filtro opcional por bairro.</param>
    /// <returns>Uma lista de objetos `CompanyResponse` que est�o dentro do raio especificado.</returns>
    [HttpGet("company/radius")]
    [SwaggerOperation(Summary = "Lista empresas por raio geogr�fico", Description = "Retorna empresas localizadas dentro de um raio especificado (em km) a partir de uma coordenada central, com filtros opcionais por cidade e bairro.")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<CompanyResponse>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ProblemDetails))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetCompaniesByRadius([FromQuery] double centerLat, [FromQuery] double centerLon, [FromQuery] double radiusKm, [FromQuery] string? city = null, [FromQuery] string? neighborhood = null)
    {
        var companies = await _getCompaniesWithinRadiusUseCase.ExecuteAsync(centerLat, centerLon, radiusKm, city, neighborhood);
        return Ok(companies);
    }
}
