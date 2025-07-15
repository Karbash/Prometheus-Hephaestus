﻿using Hephaestus.Domain.DTOs.Response;
using Hephaestus.Application.DTOs.Request;
using Hephaestus.Application.DTOs.Response;
using Hephaestus.Application.Interfaces.Administration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Hephaestus.Controllers;

/// <summary>
/// Controller para gerenciamento administrativo de empresas, vendas e logs.
/// Todas as operações requerem autenticação com a role "Admin" e a policy "RequireMfa".
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
    /// Inicializa uma nova instância do <see cref="AdministrationController"/>.
    /// </summary>
    /// <param name="getCompaniesUseCase">Caso de uso para listar empresas.</param>
    /// <param name="updateCompanyUseCase">Caso de uso para atualizar empresas.</param>
    /// <param name="salesReportUseCase">Caso de uso para relatórios de vendas.</param>
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
    /// Lista todas as empresas registradas no sistema, com opções de filtro e paginação.
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
    ///       "city": "São Paulo",
    ///       "neighborhood": "Vila Mariana",
    ///       "street": "Rua Exemplo",
    ///       "number": "123",
    ///       "latitude": -23.550520,
    ///       "longitude": -46.633308,
    ///       "slogan": "O melhor da cidade!",
    ///       "description": "Descrição da empresa."
    ///     }
    ///   ],
    ///   "totalCount": 1,
    ///   "pageNumber": 1,
    ///   "pageSize": 20
    /// }
    /// ```
    /// </remarks>
    /// <param name="isEnabled">Filtro opcional para empresas habilitadas (true) ou desabilitadas (false).</param>
    /// <param name="pageNumber">Número da página a ser retornada (padrão: 1).</param>
    /// <param name="pageSize">Número de itens por página (padrão: 20).</param>
    /// <returns>Uma lista paginada de objetos `CompanyResponse`.</returns>
    [HttpGet("company")]
    [SwaggerOperation(Summary = "Lista todas as empresas", Description = "Retorna uma lista paginada de empresas, com filtro opcional por status de habilitação (habilitadas ou desabilitadas).")]
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
    /// Atualiza as informações de uma empresa específica.
    /// </summary>
    /// <remarks>
    /// Requer **Role: Admin** e **MFA validado**.
    /// O `id` na URL deve corresponder ao `Id` no corpo da requisição.
    /// 
    /// Exemplo de requisição:
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
    ///   "slogan": "Inovação a cada passo!",
    ///   "description": "Empresa líder em tecnologia."
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
    ///   "message": "Empresa com ID 'xyz' não encontrada."
    /// }
    /// ```
    /// </remarks>
    /// <param name="id">ID da empresa a ser atualizada (GUID).</param>
    /// <param name="request">Objeto com os dados atualizados da empresa.</param>
    /// <returns>Um `NoContentResult` em caso de sucesso.</returns>
    [HttpPut("company/{id}")]
    [SwaggerOperation(Summary = "Atualiza uma empresa", Description = "Atualiza as informações de uma empresa existente. O ID na URL deve coincidir com o da empresa a ser atualizada.")]
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
    /// Gera um relatório consolidado de vendas de todas as empresas, com filtros opcionais por data.
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
    ///   "message": "Data inicial não pode ser maior que a data final."
    /// }
    /// ```
    /// </remarks>
    /// <param name="startDate">Data inicial para filtrar as vendas (opcional, formato ISO 8601).</param>
    /// <param name="endDate">Data final para filtrar as vendas (opcional, formato ISO 8601).</param>
    /// <returns>Um objeto `SalesReportResponse` contendo o relatório de vendas.</returns>
    [HttpGet("sales/admin")]
    [SwaggerOperation(Summary = "Relatório de vendas consolidado", Description = "Retorna um relatório consolidado de vendas de todas as empresas, com filtros opcionais por período.")]
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
    /// Lista logs de auditoria de ações administrativas.
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
    [SwaggerOperation(Summary = "Lista logs de auditoria", Description = "Retorna logs de auditoria de ações administrativas, com filtros opcionais por período.")]
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
    /// Lista empresas localizadas dentro de um raio geográfico específico.
    /// </summary>
    /// <remarks>
    /// Requer **Role: Admin** e **MFA validado**.
    /// 
    /// Exemplo de resposta de sucesso (Status 200 OK):
    /// ```json
    /// [
    ///   {
    ///     "id": "123e4567-e89b-12d3-a456-426614174001",
    ///     "name": "Empresa Próxima",
    ///     "email": "proxima@empresa.com",
    ///     "phoneNumber": "123456789",
    ///     "isEnabled": true,
    ///     "feeType": "Percentage",
    ///     "feeValue": 5.0,
    ///     "city": "São Paulo",
    ///     "neighborhood": "Vila Mariana",
    ///     "street": "Rua Exemplo",
    ///     "number": "123",
    ///     "latitude": -23.550520,
    ///     "longitude": -46.633308,
    ///     "slogan": "O melhor da cidade!",
    ///     "description": "Descrição da empresa."
    ///   }
    /// ]
    /// ```
    /// 
    /// Exemplo de erro (Status 400 Bad Request):
    /// ```json
    /// {
    ///   "message": "Parâmetros de localização inválidos. Latitude e Longitude devem ser fornecidos."
    /// }
    /// ```
    /// </remarks>
    /// <param name="centerLat">Latitude do centro do raio.</param>
    /// <param name="centerLon">Longitude do centro do raio.</param>
    /// <param name="radiusKm">Raio em quilômetros para a busca.</param>
    /// <param name="city">Filtro opcional por cidade.</param>
    /// <param name="neighborhood">Filtro opcional por bairro.</param>
    /// <returns>Uma lista de objetos `CompanyResponse` que estão dentro do raio especificado.</returns>
    [HttpGet("company/radius")]
    [SwaggerOperation(Summary = "Lista empresas por raio geográfico", Description = "Retorna empresas localizadas dentro de um raio especificado (em km) a partir de uma coordenada central, com filtros opcionais por cidade e bairro.")]
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