using Hephaestus.Application.DTOs.Request;
using Hephaestus.Application.DTOs.Response;
using Hephaestus.Application.Interfaces.Administration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Hephaestus.Controllers;

/// <summary>
/// Controller para gerenciamento administrativo de empresas, vendas e logs.
/// </summary>
[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Admin")]
public class AdministrationController : ControllerBase
{
    private readonly IGetCompaniesUseCase _getCompaniesUseCase;
    private readonly IUpdateCompanyUseCase _updateCompanyUseCase;
    private readonly ISalesReportUseCase _salesReportUseCase;
    private readonly IAuditLogUseCase _auditLogUseCase;
    private readonly ILogger<AdministrationController> _logger;

    public AdministrationController(
        IGetCompaniesUseCase getCompaniesUseCase,
        IUpdateCompanyUseCase updateCompanyUseCase,
        ISalesReportUseCase salesReportUseCase,
        IAuditLogUseCase auditLogUseCase,
        ILogger<AdministrationController> logger)
    {
        _getCompaniesUseCase = getCompaniesUseCase;
        _updateCompanyUseCase = updateCompanyUseCase;
        _salesReportUseCase = salesReportUseCase;
        _auditLogUseCase = auditLogUseCase;
        _logger = logger;
    }

    /// <summary>
    /// Lista todas as empresas.
    /// </summary>
    /// <param name="isEnabled">Filtro opcional para empresas habilitadas (true) ou desabilitadas (false).</param>
    /// <returns>Lista de empresas.</returns>
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
    /// <param name="id">ID da empresa a ser atualizada.</param>
    /// <param name="request">Dados atualizados da empresa.</param>
    /// <returns>Status da atualização.</returns>
    [HttpPut("company/{id}")]
    [SwaggerOperation(Summary = "Atualiza uma empresa", Description = "Atualiza configurações de uma empresa (nome, e-mail, telefone, API key, tipo de taxa, valor da taxa, status de habilitação). Cria log de auditoria. Requer autenticação de administrador.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(object))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(object))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(object))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(object))]
    public async Task<IActionResult> UpdateCompany(string id, [FromBody] CompanyRequest request)
    {
        try
        {
            await _updateCompanyUseCase.ExecuteAsync(id, request, User);
            await _auditLogUseCase.ExecuteAsync("UpdateCompany", id, $"Empresa {id} atualizada por admin.", User);
            return NoContent();
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
    /// <param name="startDate">Data inicial do relatório no formato ISO 8601 (ex.: 2024-01-01). Opcional.</param>
    /// <param name="endDate">Data final do relatório no formato ISO 8601 (ex.: 2024-12-31). Opcional.</param>
    /// <param name="tenantId">ID da empresa (tenant) para filtrar vendas. Opcional.</param>
    /// <returns>Relatório de vendas consolidado.</returns>
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
    /// <param name="adminId">ID do administrador (GUID) para filtrar logs. Opcional.</param>
    /// <param name="startDate">Data inicial do log no formato ISO 8601 (ex.: 2024-01-01). Opcional.</param>
    /// <param name="endDate">Data final do log no formato ISO 8601 (ex.: 2024-12-31). Opcional.</param>
    /// <returns>Lista de logs de auditoria.</returns>
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
}