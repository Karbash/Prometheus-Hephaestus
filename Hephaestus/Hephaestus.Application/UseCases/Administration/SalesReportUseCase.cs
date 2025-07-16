using FluentValidation.Results;
using Hephaestus.Application.Base;
using Hephaestus.Domain.DTOs.Response;
using Hephaestus.Application.Interfaces.Administration;
using Hephaestus.Application.Services;
using Hephaestus.Domain.Repositories;
using Hephaestus.Domain.Services;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace Hephaestus.Application.UseCases.Administration;

/// <summary>
/// Caso de uso para relat�rios de vendas.
/// </summary>
public class SalesReportUseCase : BaseUseCase, ISalesReportUseCase
{
    private readonly ISalesRepository _salesRepository;
    private readonly ICompanyRepository _companyRepository;
    private readonly ILoggedUserService _loggedUserService;

    /// <summary>
    /// Inicializa uma nova inst�ncia do <see cref="SalesReportUseCase"/>.
    /// </summary>
    /// <param name="salesRepository">Reposit�rio de vendas.</param>
    /// <param name="companyRepository">Reposit�rio de empresas.</param>
    /// <param name="logger">Logger.</param>
    /// <param name="exceptionHandler">Servi�o de tratamento de exce��es.</param>
    /// <param name="loggedUserService">Servi�o do usu�rio logado.</param>
    public SalesReportUseCase(
        ISalesRepository salesRepository, 
        ICompanyRepository companyRepository,
        ILogger<SalesReportUseCase> logger,
        IExceptionHandlerService exceptionHandler,
        ILoggedUserService loggedUserService)
        : base(logger, exceptionHandler)
    {
        _salesRepository = salesRepository;
        _companyRepository = companyRepository;
        _loggedUserService = loggedUserService;
    }

    /// <summary>
    /// Executa a gera��o do relat�rio de vendas.
    /// </summary>
    /// <param name="startDate">Data inicial (opcional).</param>
    /// <param name="endDate">Data final (opcional).</param>
    /// <param name="user">Usu�rio autenticado.</param>
    /// <returns>Relat�rio de vendas.</returns>
    public async Task<SalesReportResponse> ExecuteAsync(DateTime? startDate, DateTime? endDate, ClaimsPrincipal user)
    {
        return await ExecuteWithExceptionHandlingAsync(async () =>
        {
            // Valida��o de autoriza��o
            ValidateAuthorization(user);

            // Obter tenantId do usu�rio logado (se aplic�vel)
            var tenantId = GetTenantIdIfApplicable(user);

            // Valida��o dos par�metros
            await ValidateParametersAsync(tenantId, startDate, endDate);

            // Busca das vendas
            var sales = await GetSalesAsync(startDate, endDate, tenantId);

            // Gera��o do relat�rio
            return GenerateSalesReport(sales);
        }, "Gera��o de Relat�rio de Vendas");
    }

    /// <summary>
    /// Obt�m o tenantId se o usu�rio for um tenant, caso contr�rio retorna null.
    /// </summary>
    /// <param name="user">Usu�rio autenticado.</param>
    /// <returns>TenantId ou null.</returns>
    private string? GetTenantIdIfApplicable(ClaimsPrincipal user)
    {
        var userRole = user?.FindFirst(ClaimTypes.Role)?.Value;
        if (userRole == "Tenant" && user != null)
        {
            return _loggedUserService.GetTenantId(user);
        }
        return null; // Admin pode ver todos os tenants
    }

    /// <summary>
    /// Valida a autoriza��o do usu�rio.
    /// </summary>
    /// <param name="user">Usu�rio autenticado.</param>
    private void ValidateAuthorization(ClaimsPrincipal user)
    {
        var userRole = user?.FindFirst(ClaimTypes.Role)?.Value;
        ValidateAuthorization(userRole == "Admin" || userRole == "Tenant", "Apenas administradores ou tenants podem gerar relat�rios de vendas.", "Gerar Relat�rio", "Vendas");
    }

    /// <summary>
    /// Valida os par�metros de entrada.
    /// </summary>
    /// <param name="tenantId">ID do tenant.</param>
    /// <param name="startDate">Data inicial.</param>
    /// <param name="endDate">Data final.</param>
    private async Task ValidateParametersAsync(string? tenantId, DateTime? startDate, DateTime? endDate)
    {
        if (!string.IsNullOrEmpty(tenantId))
        {
            var company = await _companyRepository.GetByIdAsync(tenantId);
            EnsureEntityExists(company, "Tenant", tenantId);
        }

        if (startDate.HasValue && endDate.HasValue && startDate > endDate)
            throw new Hephaestus.Application.Exceptions.ValidationException("A data inicial n�o pode ser posterior � data final.", new ValidationResult());
    }

    /// <summary>
    /// Busca as vendas.
    /// </summary>
    /// <param name="startDate">Data inicial.</param>
    /// <param name="endDate">Data final.</param>
    /// <param name="tenantId">ID do tenant.</param>
    /// <returns>Lista de vendas.</returns>
    private async Task<IEnumerable<Domain.Entities.SalesLog>> GetSalesAsync(DateTime? startDate, DateTime? endDate, string? tenantId)
    {
        var normalizedStartDate = startDate?.ToUniversalTime();
        var normalizedEndDate = endDate?.ToUniversalTime();

        return await _salesRepository.GetSalesAsync(normalizedStartDate, normalizedEndDate, tenantId);
    }

    /// <summary>
    /// Gera o relat�rio de vendas.
    /// </summary>
    /// <param name="sales">Lista de vendas.</param>
    /// <returns>Relat�rio de vendas.</returns>
    private SalesReportResponse GenerateSalesReport(IEnumerable<Domain.Entities.SalesLog> sales)
    {
        var totalSales = sales.Sum(s => (double)s.TotalAmount);
        var totalPlatformFees = sales.Sum(s => (double)s.PlatformFee);
        var totalPromotions = sales.Count(s => !string.IsNullOrEmpty(s.PromotionId) || !string.IsNullOrEmpty(s.CouponId));

        var salesByTenant = sales
            .GroupBy(s => s.TenantId)
            .Select(g => new SalesByTenantResponse
            {
                TenantId = g.Key,
                TotalAmount = g.Sum(s => (double)s.TotalAmount),
                PlatformFee = g.Sum(s => (double)s.PlatformFee)
            })
            .ToList();

        return new SalesReportResponse
        {
            TotalSales = totalSales,
            TotalPlatformFees = totalPlatformFees,
            SalesByTenant = salesByTenant,
            TotalPromotionsApplied = totalPromotions
        };
    }
}
