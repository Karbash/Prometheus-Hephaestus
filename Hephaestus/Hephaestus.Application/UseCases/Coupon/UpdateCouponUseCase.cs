using FluentValidation;
using Hephaestus.Application.Base;
using Hephaestus.Application.DTOs.Request;
using Hephaestus.Application.Exceptions;
using Hephaestus.Application.Interfaces.Coupon;
using Hephaestus.Application.Services;
using Hephaestus.Domain.Entities;
using Hephaestus.Domain.Enum;
using Hephaestus.Domain.Repositories;
using Hephaestus.Domain.Services;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace Hephaestus.Application.UseCases.Coupon;

/// <summary>
/// Caso de uso para atualização de cupons.
/// </summary>
public class UpdateCouponUseCase : BaseUseCase, IUpdateCouponUseCase
{
    private readonly ICouponRepository _couponRepository;
    private readonly IMenuItemRepository _menuItemRepository;
    private readonly IValidator<UpdateCouponRequest> _validator;
    private readonly ILoggedUserService _loggedUserService;

    public UpdateCouponUseCase(
        ICouponRepository couponRepository,
        IMenuItemRepository menuItemRepository,
        IValidator<UpdateCouponRequest> validator,
        ILoggedUserService loggedUserService,
        ILogger<UpdateCouponUseCase> logger,
        IExceptionHandlerService exceptionHandler)
        : base(logger, exceptionHandler)
    {
        _couponRepository = couponRepository;
        _menuItemRepository = menuItemRepository;
        _validator = validator;
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

    private async Task UpdateCouponEntityAsync(Domain.Entities.Coupon coupon, UpdateCouponRequest request)
    {
        coupon.Code = request.Code;
        coupon.CustomerPhoneNumber = request.CustomerPhoneNumber;
        coupon.DiscountType = ParseDiscountType(request.DiscountType);
        coupon.DiscountValue = request.DiscountValue;
        coupon.MenuItemId = request.MenuItemId;
        coupon.MinOrderValue = request.MinOrderValue;
        coupon.StartDate = request.StartDate;
        coupon.EndDate = request.EndDate;
        coupon.IsActive = request.IsActive;
    }

    public async Task ExecuteAsync(string id, UpdateCouponRequest request, ClaimsPrincipal user)
    {
        await ExecuteWithExceptionHandlingAsync(async () =>
        {
            var tenantId = _loggedUserService.GetTenantId(user);
            
            await ValidateAsync(_validator, request);

            var coupon = await _couponRepository.GetByIdAsync(id, tenantId);
            EnsureResourceExists(coupon, "Coupon", id);

            if (request.Code != coupon.Code)
            {
                EnsureNoConflict(!await _couponRepository.CodeExistsAsync(request.Code, tenantId),
                    "Código de cupom já existe.", "Coupon", "Code", request.Code);
            }

            if (!string.IsNullOrEmpty(request.MenuItemId))
            {
                var menuItem = await _menuItemRepository.GetByIdAsync(request.MenuItemId, tenantId);
                EnsureResourceExists(menuItem, "MenuItem", request.MenuItemId);
            }

            EnsureBusinessRule(request.StartDate < request.EndDate,
                "Data de início deve ser anterior à data de término.", "DATE_RANGE_RULE");

            EnsureBusinessRule(request.DiscountValue > 0,
                "Valor do desconto deve ser maior que zero.", "DISCOUNT_VALUE_RULE");

            await UpdateCouponEntityAsync(coupon, request);

            await _couponRepository.UpdateAsync(coupon);
        }, "UpdateCoupon");
    }
}