using Hephaestus.Domain.DTOs.Response;
using Hephaestus.Application.Interfaces.Administration;
using Hephaestus.Domain.Repositories;
using Hephaestus.Application.Base;
using Microsoft.Extensions.Logging;
using Hephaestus.Application.Services;
using System.Linq;

namespace Hephaestus.Application.UseCases.Coupon;

public class GlobalCouponAdminUseCase : BaseUseCase, IGlobalCouponAdminUseCase
{
    private readonly ICouponRepository _couponRepository;

    public GlobalCouponAdminUseCase(
        ICouponRepository couponRepository,
        ILogger<GlobalCouponAdminUseCase> logger,
        IExceptionHandlerService exceptionHandler)
        : base(logger, exceptionHandler)
    {
        _couponRepository = couponRepository;
    }

    public async Task<PagedResult<CouponResponse>> ExecuteAsync(
        string? companyId = null,
        string? code = null,
        string? customerPhoneNumber = null,
        bool? isActive = null,
        int pageNumber = 1,
        int pageSize = 20,
        string? sortBy = null,
        string? sortOrder = "asc")
    {
        return await ExecuteWithExceptionHandlingAsync(async () =>
        {
            var pagedCoupons = await _couponRepository.GetAllGlobalAsync(code, companyId, null, isActive, pageNumber, pageSize, sortBy, sortOrder);
            return new PagedResult<CouponResponse>
            {
                Items = pagedCoupons.Items.Select(c => new CouponResponse
                {
                    Id = c.Id,
                    CompanyId = null, // Valor padrão, pois não existe na entidade
                    Code = c.Code,
                    CustomerId = null, // Valor padrão
                    IsActive = c.IsActive,
                    CreatedAt = c.CreatedAt,
                    UpdatedAt = c.UpdatedAt
                }).ToList(),
                TotalCount = pagedCoupons.TotalCount,
                PageNumber = pagedCoupons.PageNumber,
                PageSize = pagedCoupons.PageSize
            };
        });
    }
} 