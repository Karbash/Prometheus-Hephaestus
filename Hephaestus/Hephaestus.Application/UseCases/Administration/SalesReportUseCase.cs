using Hephaestus.Application.DTOs.Response;
using Hephaestus.Application.Interfaces.Administration;
using Hephaestus.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Hephaestus.Application.Exceptions;
using Hephaestus.Application.Base;
using Microsoft.Extensions.Logging;
using Hephaestus.Application.Services;
using FluentValidation.Results;

namespace Hephaestus.Application.UseCases.Administration;

/// <summary>
/// Caso de uso para relatórios de vendas.
/// </summary>
public class SalesReportUseCase : BaseUseCase, ISalesReportUseCase
{
    private readonly ISalesRepository _salesRepository;
    private readonly ICompanyRepository _companyRepository;

    /// <summary>
    /// Inicializa uma nova instância do <see cref="SalesReportUseCase"/>.
    /// </summary>
    /// <param name="salesRepository">Repositório de vendas.</param>
    /// <param name="companyRepository">Repositório de empresas.</param>
    /// <param name="logger">Logger.</param>
    /// <param name="exceptionHandler">Serviço de tratamento de exceções.</param>
    public SalesReportUseCase(
        ISalesRepository salesRepository, 
        ICompanyRepository companyRepository,
        ILogger<SalesReportUseCase> logger,
        IExceptionHandlerService exceptionHandler)
        : base(logger, exceptionHandler)
    {
        _salesRepository = salesRepository;
        _companyRepository = companyRepository;
    }

    /// <summary>
    /// Executa a geração do relatório de vendas.
    /// </summary>
    /// <param name="startDate">Data inicial (opcional).</param>
    /// <param name="endDate">Data final (opcional).</param>
    /// <param name="tenantId">ID do tenant (opcional).</param>
    /// <param name="user">Usuário autenticado.</param>
    /// <returns>Relatório de vendas.</returns>
    public async Task<SalesReportResponse> ExecuteAsync(DateTime? startDate, DateTime? endDate, string? tenantId, ClaimsPrincipal user)
    {
        return await ExecuteWithExceptionHandlingAsync(async () =>
        {
            // Validação de autorização
            ValidateAuthorization(user);

            // Validação dos parâmetros
            await ValidateParametersAsync(tenantId, startDate, endDate);

            // Busca das vendas
            var sales = await GetSalesAsync(startDate, endDate, tenantId);

            // Geração do relatório
            return GenerateSalesReport(sales);
        }, "Geração de Relatório de Vendas");
    }

    /// <summary>
    /// Valida a autorização do usuário.
    /// </summary>
    /// <param name="user">Usuário autenticado.</param>
    private void ValidateAuthorization(ClaimsPrincipal user)
    {
        var userRole = user?.FindFirst(ClaimTypes.Role)?.Value;
        ValidateAuthorization(userRole == "Admin", "Apenas administradores podem gerar relatórios de vendas.", "Gerar Relatório", "Vendas");
    }

    /// <summary>
    /// Valida os parâmetros de entrada.
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
            throw new Hephaestus.Application.Exceptions.ValidationException("A data inicial não pode ser posterior à data final.", new ValidationResult());
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
    /// Gera o relatório de vendas.
    /// </summary>
    /// <param name="sales">Lista de vendas.</param>
    /// <returns>Relatório de vendas.</returns>
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