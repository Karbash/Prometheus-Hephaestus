using Hephaestus.Domain.Repositories;
using Hephaestus.Application.Interfaces.Coupon;
using Hephaestus.Application.Base;
using Hephaestus.Application.Exceptions;
using Hephaestus.Application.Services;
using Hephaestus.Domain.Services;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using System;
using Hephaestus.Domain.Enum;

namespace Hephaestus.Application.UseCases.Coupon;

/// <summary>
/// Caso de uso para remoção de cupons.
/// </summary>
public class DeleteCouponUseCase : BaseUseCase, IDeleteCouponUseCase
{
    private readonly ICouponRepository _couponRepository;
    private readonly ILoggedUserService _loggedUserService;

    public DeleteCouponUseCase(
        ICouponRepository couponRepository,
        ILoggedUserService loggedUserService,
        ILogger<DeleteCouponUseCase> logger,
        IExceptionHandlerService exceptionHandler)
        : base(logger, exceptionHandler)
    {
        _couponRepository = couponRepository;
        _loggedUserService = loggedUserService;
    }

    private DiscountType ParseDiscountType(string discountTypeStr)
    {
        if (!Enum.TryParse<DiscountType>(discountTypeStr, true, out var discountType))
        {
            throw new BusinessRuleException($"Tipo de desconto inválido: {discountTypeStr}. Os valores válidos são: {string.Join(", ", Enum.GetNames(typeof(DiscountType)))}.", "DISCOUNT_TYPE_VALIDATION");
        }
        return discountType;
    }

    public async Task ExecuteAsync(string id, ClaimsPrincipal user)
    {
        await ExecuteWithExceptionHandlingAsync(async () =>
        {
            var tenantId = _loggedUserService.GetTenantId(user);
            
            var coupon = await _couponRepository.GetByIdAsync(id, tenantId);
            EnsureResourceExists(coupon, "Coupon", id);

            await _couponRepository.DeleteAsync(id, tenantId);
        }, "DeleteCoupon");
    }
}