using Hephaestus.Application.Base;
using Hephaestus.Domain.DTOs.Response;
using Hephaestus.Application.Interfaces.Coupon;
using Hephaestus.Application.Services;
using Hephaestus.Domain.DTOs.Response;
using Hephaestus.Domain.Repositories;
using Hephaestus.Domain.Services;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace Hephaestus.Application.UseCases.Coupon;

/// <summary>
/// Caso de uso para listagem de cupons.
/// </summary>
public class GetCouponsUseCase : BaseUseCase, IGetCouponsUseCase
{
    private readonly ICouponRepository _couponRepository;
    private readonly ILoggedUserService _loggedUserService;

    public GetCouponsUseCase(
        ICouponRepository couponRepository,
        ILoggedUserService loggedUserService,
        ILogger<GetCouponsUseCase> logger,
        IExceptionHandlerService exceptionHandler)
        : base(logger, exceptionHandler)
    {
        _couponRepository = couponRepository;
        _loggedUserService = loggedUserService;
    }

    public async Task<PagedResult<CouponResponse>> ExecuteAsync(ClaimsPrincipal user, bool? isActive = null, string? customerPhoneNumber = null, int pageNumber = 1, int pageSize = 20, string? sortBy = null, string? sortOrder = "asc")
    {
        return await ExecuteWithExceptionHandlingAsync(async () =>
        {
            var tenantId = _loggedUserService.GetTenantId(user);
            var pagedCoupons = await _couponRepository.GetByTenantIdAsync(tenantId, isActive, customerPhoneNumber, pageNumber, pageSize, sortBy, sortOrder);
            return new PagedResult<CouponResponse>
            {
                Items = pagedCoupons.Items.Select(c => new CouponResponse
                {
                    Id = c.Id,
                    TenantId = c.TenantId,
                    Code = c.Code,
                    DiscountType = c.DiscountType,
                    DiscountValue = c.DiscountValue,
                    MenuItemId = c.MenuItemId,
                    MinOrderValue = c.MinOrderValue ?? 0,
                    StartDate = c.StartDate,
                    EndDate = c.EndDate,
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
