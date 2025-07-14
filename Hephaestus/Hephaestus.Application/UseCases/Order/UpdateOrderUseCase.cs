using FluentValidation;
using Hephaestus.Application.Base;
using Hephaestus.Domain.DTOs.Request;
using Hephaestus.Application.Interfaces.Order;
using Hephaestus.Application.Services;
using Hephaestus.Domain.Entities;
using Hephaestus.Domain.Repositories;
using Hephaestus.Domain.Services;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace Hephaestus.Application.UseCases.Order;

public class UpdateOrderUseCase : BaseUseCase, IUpdateOrderUseCase
{
    private readonly IOrderRepository _orderRepository;
    private readonly IMenuItemRepository _menuItemRepository;
    private readonly ICouponRepository _couponRepository;
    private readonly IPromotionRepository _promotionRepository;
    private readonly IValidator<UpdateOrderRequest> _validator;
    private readonly ILoggedUserService _loggedUserService;

    public UpdateOrderUseCase(
        IOrderRepository orderRepository,
        IMenuItemRepository menuItemRepository,
        ICouponRepository couponRepository,
        IPromotionRepository promotionRepository,
        IValidator<UpdateOrderRequest> validator,
        ILoggedUserService loggedUserService,
        ILogger<UpdateOrderUseCase> logger,
        IExceptionHandlerService exceptionHandler)
        : base(logger, exceptionHandler)
    {
        _orderRepository = orderRepository;
        _menuItemRepository = menuItemRepository;
        _couponRepository = couponRepository;
        _promotionRepository = promotionRepository;
        _validator = validator;
        _loggedUserService = loggedUserService;
    }

    public async Task ExecuteAsync(string id, UpdateOrderRequest request, ClaimsPrincipal user)
    {
        await ExecuteWithExceptionHandlingAsync(async () =>
        {
            await ValidateAsync(_validator, request);

            var tenantId = _loggedUserService.GetTenantId(user);
            var order = await _orderRepository.GetByIdAsync(id, tenantId);
            EnsureResourceExists(order, "Order", id);

            var orderItems = new List<OrderItem>();
            decimal totalAmount = 0;

            foreach (var item in request.Items)
            {
                var menuItem = await _menuItemRepository.GetByIdAsync(item.MenuItemId, tenantId);
                EnsureResourceExists(menuItem, "MenuItem", item.MenuItemId);

                var orderItem = new OrderItem
                {
                    Id = Guid.NewGuid().ToString(),
                    TenantId = tenantId,
                    OrderId = id,
                    MenuItemId = item.MenuItemId,
                    Quantity = item.Quantity,
                    UnitPrice = menuItem.Price,
                    Notes = item.Notes ?? string.Empty,
                    Tags = item.Tags ?? new List<string>(),
                    AdditionalIds = item.AdditionalIds ?? new List<string>(),
                    Customizations = item.Customizations ?? new List<Customization>()
                };
                orderItems.Add(orderItem);
                totalAmount += item.Quantity * menuItem.Price;
            }

            if (!string.IsNullOrEmpty(request.CouponId))
            {
                var coupon = await _couponRepository.GetByIdAsync(request.CouponId, tenantId);
                EnsureResourceExists(coupon, "Coupon", request.CouponId);
                EnsureBusinessRule(coupon.IsActive && coupon.StartDate <= DateTime.UtcNow && coupon.EndDate >= DateTime.UtcNow,
                    "Cupom inválido ou expirado.", "COUPON_INVALID");
            }

            if (!string.IsNullOrEmpty(request.PromotionId))
            {
                var promotion = await _promotionRepository.GetByIdAsync(request.PromotionId, tenantId);
                EnsureResourceExists(promotion, "Promotion", request.PromotionId);
                EnsureBusinessRule(promotion.IsActive && promotion.StartDate <= DateTime.UtcNow && promotion.EndDate >= DateTime.UtcNow,
                    "Promoção inválida ou expirada.", "PROMOTION_INVALID");
            }

            order.CustomerPhoneNumber = request.CustomerPhoneNumber;
            order.PromotionId = request.PromotionId;
            order.CouponId = request.CouponId;
            order.Status = request.Status;
            order.PaymentStatus = request.PaymentStatus;
            order.TotalAmount = totalAmount;
            order.OrderItems = orderItems;
            order.UpdatedAt = DateTime.UtcNow;

            await _orderRepository.UpdateAsync(order);
        }, "UpdateOrder");
    }
}