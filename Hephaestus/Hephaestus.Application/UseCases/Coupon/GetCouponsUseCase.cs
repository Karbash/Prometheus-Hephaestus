using Hephaestus.Application.DTOs.Response;
using Hephaestus.Domain.Repositories;
using Hephaestus.Application.Interfaces.Coupon;
using Hephaestus.Application.Base;
using Microsoft.Extensions.Logging;
using Hephaestus.Application.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hephaestus.Application.UseCases.Coupon;

/// <summary>
/// Caso de uso para listagem de cupons.
/// </summary>
public class GetCouponsUseCase : BaseUseCase, IGetCouponsUseCase
{
    private readonly ICouponRepository _couponRepository;

    public GetCouponsUseCase(
        ICouponRepository couponRepository,
        ILogger<GetCouponsUseCase> logger,
        IExceptionHandlerService exceptionHandler)
        : base(logger, exceptionHandler)
    {
        _couponRepository = couponRepository;
    }

    public async Task<IEnumerable<CouponResponse>> ExecuteAsync(string tenantId, bool? isActive, string? customerPhoneNumber)
    {
        return await ExecuteWithExceptionHandlingAsync(async () =>
        {
            var coupons = await _couponRepository.GetByTenantIdAsync(tenantId, isActive, customerPhoneNumber);
            return coupons.Select(c => new CouponResponse
            {
                Id = c.Id,
                Code = c.Code,
                CustomerPhoneNumber = c.CustomerPhoneNumber,
                DiscountType = c.DiscountType.ToString(),
                DiscountValue = c.DiscountValue,
                MenuItemId = c.MenuItemId,
                MinOrderValue = (decimal)c.MinOrderValue,
                StartDate = c.StartDate,
                EndDate = c.EndDate,
                IsActive = c.IsActive
            });
        }, "GetCoupons");
    }
}