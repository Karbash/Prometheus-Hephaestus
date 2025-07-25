using FluentValidation;
using Hephaestus.Application.Base;
using Hephaestus.Domain.DTOs.Request;
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
/// Caso de uso para cria��o de cupons.
/// </summary>
public class CreateCouponUseCase : BaseUseCase, ICreateCouponUseCase
{
    private readonly ICouponRepository _couponRepository;
    private readonly IMenuItemRepository _menuItemRepository;
    private readonly IValidator<CreateCouponRequest> _validator;
    private readonly ILoggedUserService _loggedUserService;

    public CreateCouponUseCase(
        ICouponRepository couponRepository,
        IMenuItemRepository menuItemRepository,
        IValidator<CreateCouponRequest> validator,
        ILoggedUserService loggedUserService,
        ILogger<CreateCouponUseCase> logger,
        IExceptionHandlerService exceptionHandler)
        : base(logger, exceptionHandler)
    {
        _couponRepository = couponRepository;
        _menuItemRepository = menuItemRepository;
        _validator = validator;
        _loggedUserService = loggedUserService;
    }

    public async Task<string> ExecuteAsync(CreateCouponRequest request, ClaimsPrincipal user)
    {
        return await ExecuteWithExceptionHandlingAsync(async () =>
        {
            var tenantId = _loggedUserService.GetTenantId(user);
            
            await ValidateAsync(_validator, request);

            await ValidateBusinessRulesAsync(request, tenantId);

            var coupon = new Domain.Entities.Coupon
            {
                Id = Guid.NewGuid().ToString(),
                TenantId = tenantId,
                Code = request.Code,
                DiscountType = request.DiscountType,
                DiscountValue = request.DiscountValue,
                MenuItemId = request.MenuItemId,
                MinOrderValue = request.MinOrderValue,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                IsActive = true
            };

            await _couponRepository.AddAsync(coupon);
            return coupon.Id;
        }, "CreateCoupon");
    }

    private async Task ValidateBusinessRulesAsync(CreateCouponRequest request, string tenantId)
    {
        EnsureNoConflict(!await _couponRepository.CodeExistsAsync(request.Code, tenantId),
            "C�digo de cupom j� existe.", "Coupon", "Code", request.Code);

        if (!string.IsNullOrEmpty(request.MenuItemId))
        {
            var menuItem = await _menuItemRepository.GetByIdAsync(request.MenuItemId, tenantId);
            EnsureResourceExists(menuItem, "MenuItem", request.MenuItemId);
        }

        EnsureBusinessRule(request.StartDate < request.EndDate,
            "Data de in�cio deve ser anterior � data de t�rmino.", "DATE_RANGE_RULE");

        EnsureBusinessRule(request.DiscountValue > 0,
            "Valor do desconto deve ser maior que zero.", "DISCOUNT_VALUE_RULE");
    }
}
