using FluentValidation;
using Hephaestus.Application.Base;
using Hephaestus.Application.DTOs.Request;
using Hephaestus.Application.Interfaces.Order;
using Hephaestus.Application.Services;
using Hephaestus.Domain.Enum;
using Hephaestus.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace Hephaestus.Application.UseCases.Order;

public class CreateOrderUseCase : BaseUseCase, ICreateOrderUseCase
{
    private readonly IOrderRepository _orderRepository;
    private readonly IMenuItemRepository _menuItemRepository;
    private readonly ICouponRepository _couponRepository;
    private readonly IPromotionRepository _promotionRepository;
    private readonly IValidator<CreateOrderRequest> _validator;

    public CreateOrderUseCase(
        IOrderRepository orderRepository,
        IMenuItemRepository menuItemRepository,
        ICouponRepository couponRepository,
        IPromotionRepository promotionRepository,
        IValidator<CreateOrderRequest> validator,
        ILogger<CreateOrderUseCase> logger,
        IExceptionHandlerService exceptionHandler)
        : base(logger, exceptionHandler)
    {
        _orderRepository = orderRepository;
        _menuItemRepository = menuItemRepository;
        _couponRepository = couponRepository;
        _promotionRepository = promotionRepository;
        _validator = validator;
    }

    public async Task<string> ExecuteAsync(CreateOrderRequest request, string tenantId)
    {
        return await ExecuteWithExceptionHandlingAsync(async () =>
        {
            await ValidateAsync(_validator, request);

            foreach (var item in request.Items)
            {
                var menuItem = await _menuItemRepository.GetByIdAsync(item.MenuItemId, tenantId);
                EnsureResourceExists(menuItem, "MenuItem", item.MenuItemId);
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

            var order = new Domain.Entities.Order
            {
                Id = Guid.NewGuid().ToString(),
                TenantId = tenantId,
                CustomerPhoneNumber = request.CustomerPhoneNumber,
                TotalAmount = 0, // Calcular com base nos itens
                PlatformFee = 0, // Calcular com base na configuração da empresa
                PromotionId = request.PromotionId,
                CouponId = request.CouponId,
                Status = OrderStatus.Pending,
                PaymentStatus = PaymentStatus.Pending,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _orderRepository.AddAsync(order);
            return order.Id;
        }, "CreateOrder");
    }
}