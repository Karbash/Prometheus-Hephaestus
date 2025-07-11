using Hephaestus.Application.DTOs.Request;
using Hephaestus.Application.DTOs.Response;
using Hephaestus.Application.Interfaces.Administration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Hephaestus.Controllers;

/// <summary>
/// Controller para gerenciamento administrativo de empresas, vendas e logs.
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
    /// Lista todas as empresas.
    /// </summary>
    /// <remarks>
    /// Exemplo de resposta de sucesso:
    /// ```json
    /// [
    ///   {
    ///     "id": "123e4567-e89b-12d3-a456-426614174001",
    ///     "name": "Empresa Exemplo",
    ///     "email": "exemplo@empresa.com",
    ///     "phoneNumber": "123456789",
    ///     "isEnabled": true,
    ///     "feeType": "Percentage",
    ///     "feeValue": 5.0,
    ///     "city": "São Paulo",
    ///     "street": "Rua Exemplo",
    ///     "number": "123",
    ///     "latitude": -23.550520,
    ///     "longitude": -46.633308,
    ///     "slogan": "O melhor da cidade!",
    ///     "description": "Descrição da empresa."
    ///   }
    /// ]
    /// ```
    /// Exemplo de erro:
    /// ```json
    /// {
    ///   "error": "Erro interno do servidor"
    /// }
    /// ```
    /// </remarks>
    /// <param name="isEnabled">Filtro opcional para empresas habilitadas (true) ou desabilitadas (false).</param>
    /// <returns>Lista de empresas.</returns>
    /// <exception cref="Exception">Erro inesperado ao listar empresas.</exception>
    [HttpGet("company")]
    [SwaggerOperation(Summary = "Lista todas as empresas", Description = "Retorna uma lista de empresas com filtro opcional por status de habilitação (true para habilitadas, false para desabilitadas). Requer autenticação de administrador.")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<CompanyResponse>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(object))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(object))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(object))]
    public async Task<IActionResult> GetCompanies([FromQuery] bool? isEnabled = null)
    {
        try
        {
            var companies = await _getCompaniesUseCase.ExecuteAsync(isEnabled);
            return Ok(companies);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao listar empresas.");
            return StatusCode(500, new { error = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Atualiza configurações de uma empresa.
    /// </summary>
    /// <remarks>
    /// Exemplo de corpo da requisição:
    /// ```json
    /// {
    ///   "id": "123e4567-e89b-12d3-a456-426614174001",
    ///   "name": "Empresa Atualizada",
    ///   "email": "atualizada@empresa.com",
    ///   "phoneNumber": "987654321",
    ///   "apiKey": "xyz789",
    ///   "feeType": "Percentage",
    ///   "feeValue": 5.0,
    ///   "isEnabled": true,
    ///   "city": "Rio de Janeiro",
    ///   "street": "Avenida Nova",
    ///   "number": "456",
    ///   "latitude": -22.906847,
    ///   "longitude": -43.172896,
    ///   "slogan": "Novo slogan!",
    ///   "description": "Nova descrição."
    /// }
    /// ```
    /// Exemplo de erro:
    /// ```json
    /// {
    ///   "error": "Empresa não encontrada."
    /// }
    /// ```
    /// </remarks>
    /// <param name="id">ID da empresa a ser atualizada.</param>
    /// <param name="request">Dados atualizados da empresa.</param>
    /// <returns>Status de sucesso (204 No Content).</returns>
    /// <exception cref="ArgumentNullException">Se request for nulo.</exception>
    /// <exception cref="KeyNotFoundException">Se a empresa não for encontrada.</exception>
    /// <exception cref="InvalidOperationException">Se e-mail ou telefone já estiver registrado ou usuário não for administrador.</exception>
    [HttpPut("company/{id}")]
    [SwaggerOperation(Summary = "Atualiza uma empresa", Description = "Atualiza configurações de uma empresa (nome, e-mail, telefone, API key, tipo de taxa, valor da taxa, status de habilitação, cidade, rua, número, latitude, longitude, slogan, descrição). Requer autenticação de administrador com MFA validado.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(object))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(object))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(object))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(object))]
    public async Task<IActionResult> UpdateCompany(string id, [FromBody] UpdateCompanyRequest request)
    {
        try
        {
            if (request.Id != id)
                return BadRequest(new { error = "ID no corpo da requisição deve corresponder ao ID na URL." });

            await _updateCompanyUseCase.ExecuteAsync(id, request, User);
            return NoContent();
        }
        catch (ArgumentNullException ex)
        {
            _logger.LogWarning(ex, "Erro ao atualizar empresa {Id}: Requisição nula.", id);
            return BadRequest(new { error = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Empresa {Id} não encontrada.", id);
            return NotFound(new { error = "Empresa não encontrada." });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Erro ao atualizar empresa {Id}.", id);
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar empresa {Id}.", id);
            return StatusCode(500, new { error = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Relatório consolidado de vendas de todas as empresas.
    /// </summary>
    /// <remarks>
    /// Exemplo de resposta de sucesso:
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
    ///     }
    ///   ]
    /// }
    /// ```
    /// Exemplo de erro:
    /// ```json
    /// {
    ///   "error": "Datas inválidas."
    /// }
    /// ```
    /// </remarks>
    /// <param name="startDate">Data inicial do relatório no formato ISO 8601 (ex.: 2024-01-01). Opcional.</param>
    /// <param name="endDate">Data final do relatório no formato ISO 8601 (ex.: 2024-12-31). Opcional.</param>
    /// <param name="tenantId">ID da empresa (tenant) para filtrar vendas. Opcional.</param>
    /// <returns>Relatório de vendas consolidado.</returns>
    /// <exception cref="InvalidOperationException">Datas inválidas ou tenantId inválido.</exception>
    [HttpGet("sales/admin")]
    [SwaggerOperation(Summary = "Relatório de vendas", Description = "Retorna relatório consolidado de vendas de todas as empresas, com filtros opcionais por data (ISO 8601) e tenantId (GUID válido). Requer autenticação de administrador.")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SalesReportResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(object))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(object))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(object))]
    public async Task<IActionResult> GetSalesReport([FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null, [FromQuery] string? tenantId = null)
    {
        try
        {
            var report = await _salesReportUseCase.ExecuteAsync(startDate, endDate, tenantId, User);
            return Ok(report);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Erro ao gerar relatório de vendas.");
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao gerar relatório de vendas.");
            return StatusCode(500, new { error = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Lista logs de auditoria de ações administrativas.
    /// </summary>
    /// <remarks>
    /// Exemplo de resposta de sucesso:
    /// ```json
    /// [
    ///   {
    ///     "id": "123e4567-e89b-12d3-a456-426614174001",
    ///     "userId": "456e7890-e89b-12d3-a456-426614174002",
    ///     "action": "UpdateCompany",
    ///     "entityId": "123e4567-e89b-12d3-a456-426614174001",
    ///     "details": "Empresa 123e4567-e89b-12d3-a456-426614174001 atualizada por admin.",
    ///     "createdAt": "2024-01-01T12:00:00Z"
    ///   }
    /// ]
    /// ```
    /// Exemplo de erro:
    /// ```json
    /// {
    ///   "error": "Erro interno do servidor"
    /// }
    /// ```
    /// </remarks>
    /// <param name="adminId">ID do administrador (GUID) para filtrar logs. Opcional.</param>
    /// <param name="startDate">Data inicial do log no formato ISO 8601 (ex.: 2024-01-01). Opcional.</param>
    /// <param name="endDate">Data final do log no formato ISO 8601 (ex.: 2024-12-31). Opcional.</param>
    /// <returns>Lista de logs de auditoria.</returns>
    /// <exception cref="InvalidOperationException">Parâmetros inválidos.</exception>
    [HttpGet("audit-log")]
    [SwaggerOperation(Summary = "Lista logs de auditoria", Description = "Retorna logs de auditoria de ações administrativas, com filtros opcionais por adminId (GUID válido) e data (ISO 8601). Requer autenticação de administrador.")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<AuditLogResponse>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(object))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(object))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(object))]
    public async Task<IActionResult> GetAuditLogs([FromQuery] string? adminId = null, [FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
    {
        try
        {
            var logs = await _auditLogUseCase.ExecuteAsync(adminId, startDate, endDate, User);
            return Ok(logs);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Erro ao listar logs de auditoria.");
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao listar logs de auditoria.");
            return StatusCode(500, new { error = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Lista empresas dentro de um raio a partir de uma coordenada.
    /// </summary>
    /// <remarks>
    /// Exemplo de requisição:
    /// ```http
    /// GET /api/administration/company/radius?centerLat=-22.906847&centerLon=-43.172896&radiusKm=10
    /// ```
    /// Exemplo de resposta de sucesso:
    /// ```json
    /// [
    ///   {
    ///     "id": "123e4567-e89b-12d3-a456-426614174001",
    ///     "name": "Empresa Exemplo",
    ///     "email": "exemplo@empresa.com",
    ///     "phoneNumber": "123456789",
    ///     "isEnabled": true,
    ///     "feeType": "Percentage",
    ///     "feeValue": 5.0,
    ///     "city": "Rio de Janeiro",
    ///     "street": "Avenida Nova",
    ///     "number": "456",
    ///     "latitude": -22.906847,
    ///     "longitude": -43.172896,
    ///     "slogan": "Novo slogan!",
    ///     "description": "Nova descrição."
    ///   }
    /// ]
    /// ```
    /// Exemplo de erro:
    /// ```json
    /// {
    ///   "error": "Raio deve ser maior que zero."
    /// }
    /// ```
    /// </remarks>
    /// <param name="centerLat">Latitude do ponto central (-90 a 90).</param>
    /// <param name="centerLon">Longitude do ponto central (-180 a 180).</param>
    /// <param name="radiusKm">Raio em quilômetros (maior que 0).</param>
    /// <returns>Lista de empresas dentro do raio.</returns>
    /// <exception cref="ArgumentException">Se latitude, longitude ou raio forem inválidos.</exception>
    [HttpGet("company/radius")]
    [SwaggerOperation(Summary = "Lista empresas por raio", Description = "Retorna empresas dentro de um raio (em km) a partir de uma coordenada (latitude, longitude). Requer autenticação de administrador com MFA validado.")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<CompanyResponse>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(object))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(object))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(object))]
    public async Task<IActionResult> GetCompaniesByRadius([FromQuery] double centerLat, [FromQuery] double centerLon, [FromQuery] double radiusKm)
    {
        try
        {
            var companies = await _getCompaniesWithinRadiusUseCase.ExecuteAsync(centerLat, centerLon, radiusKm);
            return Ok(companies);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Erro de validação ao listar empresas por raio: {Message}", ex.Message);
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao listar empresas por raio.");
            return StatusCode(500, new { error = "Erro interno do servidor" });
        }
    }
}