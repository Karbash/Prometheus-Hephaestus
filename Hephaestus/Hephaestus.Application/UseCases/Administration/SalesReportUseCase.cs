using Hephaestus.Application.DTOs.Response;
using Hephaestus.Application.Interfaces.Administration;
using Hephaestus.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Hephaestus.Application.UseCases.Administration;

public class SalesReportUseCase : ISalesReportUseCase
{
    private readonly ISalesRepository _salesRepository;
    private readonly ICompanyRepository _companyRepository;

    public SalesReportUseCase(ISalesRepository salesRepository, ICompanyRepository companyRepository)
    {
        _salesRepository = salesRepository;
        _companyRepository = companyRepository;
    }

    public async Task<SalesReportResponse> ExecuteAsync(DateTime? startDate, DateTime? endDate, string? tenantId, ClaimsPrincipal user)
    {
        var userRole = user?.FindFirst(ClaimTypes.Role)?.Value;
        if (userRole != "Admin")
            throw new InvalidOperationException("Apenas administradores podem gerar relatórios de vendas.");

        // Validar tenantId, se fornecido
        if (!string.IsNullOrEmpty(tenantId))
        {
            var company = await _companyRepository.GetByIdAsync(tenantId);
            if (company == null)
                throw new InvalidOperationException("Tenant inválido.");
        }

        // Validar datas
        if (startDate.HasValue && endDate.HasValue && startDate > endDate)
            throw new InvalidOperationException("A data inicial não pode ser posterior à data final.");

        // Normalizar datas para UTC
        var normalizedStartDate = startDate?.ToUniversalTime();
        var normalizedEndDate = endDate?.ToUniversalTime();

        var sales = await _salesRepository.GetSalesAsync(normalizedStartDate, normalizedEndDate, tenantId);

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