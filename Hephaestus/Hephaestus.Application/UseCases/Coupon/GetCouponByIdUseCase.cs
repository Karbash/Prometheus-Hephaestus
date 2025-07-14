using Hephaestus.Application.DTOs.Response;
using Hephaestus.Domain.Entities;
using Hephaestus.Domain.Repositories;
using Hephaestus.Application.Interfaces.Coupon;
using Hephaestus.Application.Base;
using Hephaestus.Application.Exceptions;
using Hephaestus.Application.Services;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Hephaestus.Application.UseCases.Coupon;

/// <summary>
/// Caso de uso para obtenção de cupom por ID.
/// </summary>
public class GetCouponByIdUseCase : BaseUseCase, IGetCouponByIdUseCase
{
    private readonly ICouponRepository _couponRepository;

    public GetCouponByIdUseCase(
        ICouponRepository couponRepository,
        ILogger<GetCouponByIdUseCase> logger,
        IExceptionHandlerService exceptionHandler)
        : base(logger, exceptionHandler)
    {
        _couponRepository = couponRepository;
    }

    public async Task<CouponResponse> ExecuteAsync(string id, string tenantId)
    {
        return await ExecuteWithExceptionHandlingAsync(async () =>
        {
            var coupon = await _couponRepository.GetByIdAsync(id, tenantId);
            EnsureResourceExists(coupon, "Coupon", id);

            return new CouponResponse
            {
                Id = coupon.Id,
                Code = coupon.Code,
                CustomerPhoneNumber = coupon.CustomerPhoneNumber,
                DiscountType = coupon.DiscountType.ToString(),
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