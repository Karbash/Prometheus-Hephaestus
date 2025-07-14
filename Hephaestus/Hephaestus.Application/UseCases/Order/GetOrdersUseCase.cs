using Hephaestus.Application.Base;
using Hephaestus.Application.DTOs.Response;
using Hephaestus.Application.Interfaces.Order;
using Hephaestus.Application.Services;
using Hephaestus.Domain.Repositories;
using Hephaestus.Domain.Services;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace Hephaestus.Application.UseCases.Order;

public class GetOrdersUseCase : BaseUseCase, IGetOrdersUseCase
{
    private readonly IOrderRepository _orderRepository;
    private readonly ILoggedUserService _loggedUserService;

    public GetOrdersUseCase(
        IOrderRepository orderRepository,
        ILoggedUserService loggedUserService,
        ILogger<GetOrdersUseCase> logger,
        IExceptionHandlerService exceptionHandler)
        : base(logger, exceptionHandler)
    {
        _orderRepository = orderRepository;
        _loggedUserService = loggedUserService;
    }

    public async Task<IEnumerable<OrderResponse>> ExecuteAsync(ClaimsPrincipal user, string? customerPhoneNumber, string? status)
    {
        return await ExecuteWithExceptionHandlingAsync(async () =>
        {
            var tenantId = _loggedUserService.GetTenantId(user);
            var orders = await _orderRepository.GetByTenantIdAsync(tenantId, customerPhoneNumber, status);
            return orders.Select(o => new OrderResponse
            {
                Id = o.Id,
                CustomerPhoneNumber = o.CustomerPhoneNumber,
                TotalAmount = o.TotalAmount,
                PlatformFee = o.PlatformFee,
                PromotionId = o.PromotionId,
                CouponId = o.CouponId,
                Status = o.Status,
                PaymentStatus = o.PaymentStatus,
                CreatedAt = o.CreatedAt,
                UpdatedAt = o.UpdatedAt
            });
        }, "GetOrders");
    }
}