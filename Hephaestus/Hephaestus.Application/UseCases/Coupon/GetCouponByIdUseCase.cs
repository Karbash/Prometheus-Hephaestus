using Hephaestus.Domain.DTOs.Response;
using Hephaestus.Domain.Entities;
using Hephaestus.Domain.Repositories;
using Hephaestus.Application.Interfaces.Coupon;
using Hephaestus.Application.Base;
using Hephaestus.Application.Exceptions;
using Hephaestus.Application.Services;
using Hephaestus.Domain.Services;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using Hephaestus.Domain.Enum;

namespace Hephaestus.Application.UseCases.Coupon;

/// <summary>
/// Caso de uso para obtenção de cupom por ID.
/// </summary>
public class GetCouponByIdUseCase : BaseUseCase, IGetCouponByIdUseCase
{
    private readonly ICouponRepository _couponRepository;
    private readonly ILoggedUserService _loggedUserService;

    public GetCouponByIdUseCase(
        ICouponRepository couponRepository,
        ILoggedUserService loggedUserService,
        ILogger<GetCouponByIdUseCase> logger,
        IExceptionHandlerService exceptionHandler)
        : base(logger, exceptionHandler)
    {
        _couponRepository = couponRepository;
        _loggedUserService = loggedUserService;
    }

    public async Task<CouponResponse> ExecuteAsync(string id, ClaimsPrincipal user)
    {
        return await ExecuteWithExceptionHandlingAsync(async () =>
        {
            var tenantId = _loggedUserService.GetTenantId(user);
            
            var coupon = await _couponRepository.GetByIdAsync(id, tenantId);
            EnsureResourceExists(coupon, "Coupon", id);

            return new CouponResponse
            {
                Id = coupon.Id,
                Code = coupon.Code,
                CustomerPhoneNumber = coupon.CustomerPhoneNumber,
                DiscountType = coupon.DiscountType,
                DiscountValue = coupon.DiscountValue,
                MenuItemId = coupon.MenuItemId,
                MinOrderValue = (decimal)coupon.MinOrderValue,
                StartDate = coupon.StartDate,
                EndDate = coupon.EndDate,
                IsActive = coupon.IsActive
            };
        }, "GetCouponById");
    }
}
