using FluentValidation;
using Hephaestus.Application.Base;
using Hephaestus.Application.Interfaces.Payment;
using Hephaestus.Domain.DTOs.Request;
using Hephaestus.Domain.DTOs.Response;
using Hephaestus.Application.Services;
using Hephaestus.Domain.Entities;
using Hephaestus.Domain.Enum;
using Hephaestus.Domain.Repositories;
using Hephaestus.Domain.Services;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace Hephaestus.Application.UseCases;

public class ProcessPaymentUseCase : BaseUseCase, IProcessPaymentUseCase
{
    private readonly IOrderRepository _orderRepository;
    private readonly ICompanyRepository _companyRepository;
    private readonly IPromotionRepository _promotionRepository;
    private readonly ICouponRepository _couponRepository;
    private readonly ISalesRepository _salesRepository;
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly IValidator<PaymentRequest> _validator;
    private readonly ILoggedUserService _loggedUserService;

    public ProcessPaymentUseCase(
        IOrderRepository orderRepository,
        ICompanyRepository companyRepository,
        IPromotionRepository promotionRepository,
        ICouponRepository couponRepository,
        ISalesRepository salesRepository,
        IAuditLogRepository auditLogRepository,
        IValidator<PaymentRequest> validator,
        ILoggedUserService loggedUserService,
        ILogger<ProcessPaymentUseCase> logger,
        IExceptionHandlerService exceptionHandler)
        : base(logger, exceptionHandler)
    {
        _orderRepository = orderRepository;
        _companyRepository = companyRepository;
        _promotionRepository = promotionRepository;
        _couponRepository = couponRepository;
        _salesRepository = salesRepository;
        _auditLogRepository = auditLogRepository;
        _validator = validator;
        _loggedUserService = loggedUserService;
    }

    public async Task<PaymentResponse> ExecuteAsync(PaymentRequest request, ClaimsPrincipal user)
    {
        return await ExecuteWithExceptionHandlingAsync(async () =>
        {
            // Obter tenantId do usuário logado
            var tenantId = _loggedUserService.GetTenantId(user);

            await ValidateAsync(_validator, request);
            await EnsureAuthorized(request.OrderId, tenantId);
            await EnsureResourceExists(request.OrderId, tenantId);
            await EnsureNoConflict(request.OrderId, tenantId);

            var order = await _orderRepository.GetByIdAsync(request.OrderId, tenantId);
            EnsureResourceExists(order, "Order", request.OrderId);

            var company = await _companyRepository.GetByIdAsync(tenantId);
            EnsureResourceExists(company, "Company", tenantId);
            EnsureBusinessRule(company.IsEnabled, "Empresa desativada.", "CompanyStatus");

            decimal platformFee = CalculatePlatformFee(order.TotalAmount, company.FeeType, company.FeeValue);
            decimal finalAmount = order.TotalAmount;

            if (!string.IsNullOrEmpty(order.PromotionId))
            {
                var promotion = await _promotionRepository.GetByIdAsync(order.PromotionId, tenantId);
                EnsureBusinessRule(promotion is not null && promotion.IsActive, "Promoção inválida.", "InvalidPromotion");
                finalAmount = ApplyDiscount(finalAmount, promotion!.DiscountType, promotion.DiscountValue, promotion.MenuItemId, order, tenantId);
            }

            if (!string.IsNullOrEmpty(order.CouponId))
            {
                var coupon = await _couponRepository.GetByIdAsync(order.CouponId, tenantId);
                EnsureBusinessRule(coupon is not null && coupon.IsActive, "Cupom inválido.", "InvalidCoupon");
                finalAmount = ApplyDiscount(finalAmount, coupon!.DiscountType, coupon.DiscountValue, coupon.MenuItemId, order, tenantId);
            }

            order.PaymentStatus = PaymentStatus.Completed;
            order.UpdatedAt = DateTime.UtcNow;
            await _orderRepository.UpdateAsync(order);

            var salesLog = new SalesLog
            {
                Id = Guid.NewGuid().ToString(),
                TenantId = tenantId,
                CustomerPhoneNumber = order.CustomerPhoneNumber,
                OrderId = order.Id,
                TotalAmount = finalAmount,
                PlatformFee = platformFee,
                PromotionId = order.PromotionId,
                CouponId = order.CouponId,
                PaymentStatus = PaymentStatus.Completed,
                CreatedAt = DateTime.UtcNow
            };
            await _salesRepository.AddAsync(salesLog);

            await _auditLogRepository.AddAsync(new AuditLog
            {
                Id = Guid.NewGuid().ToString(),
                TenantId = tenantId,
                Action = "Pagamento processado",
                EntityId = order.Id,
                EntityType = "Order",
                Details = $"Pagamento de {finalAmount:F2} processado para o pedido {order.Id}.",
                CreatedAt = DateTime.UtcNow
            });

            return new PaymentResponse
            {
                OrderId = order.Id,
                TotalAmount = finalAmount,
                PlatformFee = platformFee,
                PaymentStatus = order.PaymentStatus
            };
        }, "ProcessPayment");
    }

    private decimal CalculatePlatformFee(decimal totalAmount, FeeType feeType, decimal feeValue)
    {
        return feeType == FeeType.Percentage ? totalAmount * (feeValue / 100) : feeValue;
    }

    private decimal ApplyDiscount(decimal total, DiscountType discountType, decimal discountValue, string? menuItemId, Domain.Entities.Order order, string tenantId)
    {
        if (discountType == DiscountType.Percentage)
        {
            return total * (1 - discountValue / 100);
        }
        else if (discountType == DiscountType.Fixed)
        {
            return Math.Max(0, total - discountValue);
        }
        else if (discountType == DiscountType.FreeItem && !string.IsNullOrEmpty(menuItemId))
        {
            var orderItem = order.OrderItems.FirstOrDefault(oi => oi.MenuItemId == menuItemId && oi.TenantId == tenantId);
            if (orderItem != null)
            {
                return Math.Max(0, total - orderItem.UnitPrice);
            }
        }
        return total;
    }

    private async Task EnsureResourceExists(string orderId, string tenantId)
    {
        var order = await _orderRepository.GetByIdAsync(orderId, tenantId);
        EnsureResourceExists(order, "Order", orderId);
    }

    private async Task EnsureNoConflict(string orderId, string tenantId)
    {
        var order = await _orderRepository.GetByIdAsync(orderId, tenantId);
        EnsureNoConflict(order?.PaymentStatus != PaymentStatus.Completed, "Pagamento já processado.", "Order", "PaymentStatus", orderId);
    }

    private async Task EnsureAuthorized(string orderId, string tenantId)
    {
        var order = await _orderRepository.GetByIdAsync(orderId, tenantId);
        EnsureAuthorized(order != null, "Acesso não autorizado ao pedido.", "ProcessPayment", orderId);
    }
}