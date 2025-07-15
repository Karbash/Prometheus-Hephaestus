using FluentValidation;
using Hephaestus.Application.Base;
using Hephaestus.Domain.DTOs.Request;
using Hephaestus.Application.Interfaces.Order;
using Hephaestus.Application.Services;
using Hephaestus.Domain.Entities;
using Hephaestus.Domain.Enum;
using Hephaestus.Domain.Repositories;
using Hephaestus.Domain.Services;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using Hephaestus.Application.Exceptions;

namespace Hephaestus.Application.UseCases.Order;

public class CreateOrderUseCase : BaseUseCase, ICreateOrderUseCase
{
    private readonly IOrderRepository _orderRepository;
    private readonly IMenuItemRepository _menuItemRepository;
    private readonly ICouponRepository _couponRepository;
    private readonly IPromotionRepository _promotionRepository;
    private readonly IValidator<CreateOrderRequest> _validator;
    private readonly ILoggedUserService _loggedUserService;
    private readonly ICompanyRepository _companyRepository;

    public CreateOrderUseCase(
        IOrderRepository orderRepository,
        IMenuItemRepository menuItemRepository,
        ICouponRepository couponRepository,
        IPromotionRepository promotionRepository,
        IValidator<CreateOrderRequest> validator,
        ILoggedUserService loggedUserService,
        ICompanyRepository companyRepository,
        ILogger<CreateOrderUseCase> logger,
        IExceptionHandlerService exceptionHandler)
        : base(logger, exceptionHandler)
    {
        _orderRepository = orderRepository;
        _menuItemRepository = menuItemRepository;
        _couponRepository = couponRepository;
        _promotionRepository = promotionRepository;
        _validator = validator;
        _loggedUserService = loggedUserService;
        _companyRepository = companyRepository;
    }

    public async Task<string> ExecuteAsync(CreateOrderRequest request, ClaimsPrincipal user)
    {
        return await ExecuteWithExceptionHandlingAsync(async () =>
        {
            await ValidateAsync(_validator, request);

            var tenantId = _loggedUserService.GetTenantId(user);
            decimal totalAmount = 0;
            var orderId = Guid.NewGuid().ToString();
            var orderItems = new List<OrderItem>();

            foreach (var item in request.Items)
            {
                var menuItem = await _menuItemRepository.GetByIdAsync(item.MenuItemId, tenantId);
                EnsureResourceExists(menuItem, "MenuItem", item.MenuItemId);

                var orderItem = new OrderItem
                {
                    Id = Guid.NewGuid().ToString(),
                    TenantId = tenantId,
                    OrderId = orderId, // Corrigido: todos os itens usam o mesmo OrderId
                    MenuItemId = item.MenuItemId,
                    Quantity = item.Quantity,
                    UnitPrice = menuItem.Price,
                    Notes = item.Notes ?? string.Empty,
                    Tags = item.Tags ?? new List<string>(),
                    AdditionalIds = item.AdditionalIds ?? new List<string>(),
                    Customizations = item.Customizations?
                        .Select(c => new Customization
                        {
                            Type = c.Type,
                            Value = c.Value
                        }).ToList() ?? new List<Customization>()
                };
                orderItems.Add(orderItem);
                totalAmount += item.Quantity * menuItem.Price;
            }

            decimal discount = 0;
            bool usedCoupon = false;
            bool usedPromotion = false;

            // Validação e aplicação de cupom
            if (!string.IsNullOrEmpty(request.CouponId))
            {
                var coupon = await _couponRepository.GetByIdAsync(request.CouponId, tenantId);
                EnsureResourceExists(coupon, "Coupon", request.CouponId);
                EnsureBusinessRule(coupon.IsActive && coupon.StartDate <= DateTime.UtcNow && coupon.EndDate >= DateTime.UtcNow,
                    "Cupom inválido ou expirado.", "COUPON_INVALID");

                // Limites de uso
                var totalUses = await _couponRepository.GetUsageCountAsync(coupon.Id, tenantId);
                var usesByCustomer = await _couponRepository.GetUsageCountByCustomerAsync(coupon.Id, tenantId, request.CustomerPhoneNumber);
                if (coupon.MaxTotalUses.HasValue && totalUses >= coupon.MaxTotalUses.Value)
                    throw new BusinessRuleException("Limite máximo de usos do cupom atingido.", "COUPON_MAX_TOTAL_USES");
                if (coupon.MaxUsesPerCustomer.HasValue && usesByCustomer >= coupon.MaxUsesPerCustomer.Value)
                    throw new BusinessRuleException("Limite máximo de usos do cupom por cliente atingido.", "COUPON_MAX_USES_PER_CUSTOMER");

                // Aplica desconto
                if (coupon.DiscountType == Domain.Enum.DiscountType.Percentage)
                    discount = totalAmount * (coupon.DiscountValue / 100);
                else
                    discount = coupon.DiscountValue;
                usedCoupon = true;
            }
            // Se não usou cupom, tenta promoção
            else if (!string.IsNullOrEmpty(request.PromotionId))
            {
                var promotion = await _promotionRepository.GetByIdAsync(request.PromotionId, tenantId);
                EnsureResourceExists(promotion, "Promotion", request.PromotionId);
                EnsureBusinessRule(promotion.IsActive && promotion.StartDate <= DateTime.UtcNow && promotion.EndDate >= DateTime.UtcNow,
                    "Promoção inválida ou expirada.", "PROMOTION_INVALID");

                // Limites de uso
                var totalUses = await _promotionRepository.GetUsageCountAsync(promotion.Id, tenantId);
                var usesByCustomer = await _promotionRepository.GetUsageCountByCustomerAsync(promotion.Id, tenantId, request.CustomerPhoneNumber);
                if (promotion.MaxTotalUses.HasValue && totalUses >= promotion.MaxTotalUses.Value)
                    throw new BusinessRuleException("Limite máximo de usos da promoção atingido.", "PROMOTION_MAX_TOTAL_USES");
                if (promotion.MaxUsesPerCustomer.HasValue && usesByCustomer >= promotion.MaxUsesPerCustomer.Value)
                    throw new BusinessRuleException("Limite máximo de usos da promoção por cliente atingido.", "PROMOTION_MAX_USES_PER_CUSTOMER");

                // Aplica desconto
                if (promotion.DiscountType == Domain.Enum.DiscountType.Percentage)
                    discount = totalAmount * (promotion.DiscountValue / 100);
                else
                    discount = promotion.DiscountValue;
                usedPromotion = true;
            }

            var company = await _companyRepository.GetByIdAsync(tenantId);
            EnsureResourceExists(company, "Company", tenantId);

            // Calcula a taxa de plataforma
            var platformFee = company.FeeType == FeeType.Percentage ? totalAmount * (company.FeeValue / 100) : company.FeeValue;

            // Aplica desconto ao total
            var finalAmount = Math.Max(0, totalAmount - discount);

            var order = new Domain.Entities.Order
            {
                Id = orderId,
                TenantId = tenantId,
                CustomerPhoneNumber = request.CustomerPhoneNumber,
                TotalAmount = finalAmount,
                PlatformFee = platformFee,
                PromotionId = usedPromotion ? request.PromotionId : null,
                CouponId = usedCoupon ? request.CouponId : null,
                Status = OrderStatus.Pending,
                PaymentStatus = PaymentStatus.Pending,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                OrderItems = orderItems
            };

            await _orderRepository.AddAsync(order);

            // Registrar uso de cupom/promoção
            if (usedCoupon)
            {
                await _couponRepository.AddUsageAsync(new CouponUsage
                {
                    TenantId = tenantId,
                    CouponId = request.CouponId!,
                    CustomerPhoneNumber = request.CustomerPhoneNumber,
                    OrderId = orderId,
                    UsedAt = DateTime.UtcNow
                });
            }
            else if (usedPromotion)
            {
                await _promotionRepository.AddUsageAsync(new PromotionUsage
                {
                    TenantId = tenantId,
                    PromotionId = request.PromotionId!,
                    CustomerPhoneNumber = request.CustomerPhoneNumber,
                    OrderId = orderId,
                    UsedAt = DateTime.UtcNow
                });
            }

            return order.Id;
        }, "CreateOrder");
    }
}