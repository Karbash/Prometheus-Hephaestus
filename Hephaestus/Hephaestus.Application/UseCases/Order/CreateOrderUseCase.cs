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

            var company = await _companyRepository.GetByIdAsync(tenantId);
            EnsureResourceExists(company, "Company", tenantId);

            // Calcula a taxa de plataforma
            var platformFee = company.FeeType == FeeType.Percentage ? totalAmount * (company.FeeValue / 100) : company.FeeValue;

            var order = new Domain.Entities.Order
            {
                Id = orderId,
                TenantId = tenantId,
                CustomerPhoneNumber = request.CustomerPhoneNumber,
                TotalAmount = totalAmount,
                PlatformFee = platformFee,
                PromotionId = request.PromotionId,
                CouponId = request.CouponId,
                Status = OrderStatus.Pending,
                PaymentStatus = PaymentStatus.Pending,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                OrderItems = orderItems
            };

            await _orderRepository.AddAsync(order);
            return order.Id;
        }, "CreateOrder");
    }
}