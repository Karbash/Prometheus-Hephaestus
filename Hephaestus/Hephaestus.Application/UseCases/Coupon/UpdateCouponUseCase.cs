using FluentValidation;
using Hephaestus.Application.Base;
using Hephaestus.Application.DTOs.Request;
using Hephaestus.Application.Exceptions;
using Hephaestus.Application.Interfaces.Coupon;
using Hephaestus.Application.Services;
using Hephaestus.Domain.Entities;
using Hephaestus.Domain.Enum;
using Hephaestus.Domain.Repositories;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Hephaestus.Application.UseCases.Coupon;

/// <summary>
/// Caso de uso para atualização de cupons.
/// </summary>
public class UpdateCouponUseCase : BaseUseCase, IUpdateCouponUseCase
{
    private readonly ICouponRepository _couponRepository;
    private readonly IMenuItemRepository _menuItemRepository;
    private readonly IValidator<UpdateCouponRequest> _validator;

    public UpdateCouponUseCase(
        ICouponRepository couponRepository,
        IMenuItemRepository menuItemRepository,
        IValidator<UpdateCouponRequest> validator,
        ILogger<UpdateCouponUseCase> logger,
        IExceptionHandlerService exceptionHandler)
        : base(logger, exceptionHandler)
    {
        _couponRepository = couponRepository;
        _menuItemRepository = menuItemRepository;
        _validator = validator;
    }

    public async Task ExecuteAsync(string id, UpdateCouponRequest request, string tenantId)
    {
        await ExecuteWithExceptionHandlingAsync(async () =>
        {
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

            coupon.Code = request.Code;
            coupon.CustomerPhoneNumber = request.CustomerPhoneNumber;
            coupon.DiscountType = Enum.Parse<DiscountType>(request.DiscountType);
            coupon.DiscountValue = request.DiscountValue;
            coupon.MenuItemId = request.MenuItemId;
            coupon.MinOrderValue = request.MinOrderValue;
            coupon.StartDate = request.StartDate;
            coupon.EndDate = request.EndDate;
            coupon.IsActive = request.IsActive;

            await _couponRepository.UpdateAsync(coupon);
        }, "UpdateCoupon");
    }
}