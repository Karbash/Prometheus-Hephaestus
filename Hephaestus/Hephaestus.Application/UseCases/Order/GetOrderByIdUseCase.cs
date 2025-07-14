using Hephaestus.Application.Base;
using Hephaestus.Application.DTOs.Response;
using Hephaestus.Application.Interfaces.Order;
using Hephaestus.Application.Services;
using Hephaestus.Domain.Repositories;
using Hephaestus.Domain.Services;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace Hephaestus.Application.UseCases.Order;

public class GetOrderByIdUseCase : BaseUseCase, IGetOrderByIdUseCase
{
    private readonly IOrderRepository _orderRepository;
    private readonly ILoggedUserService _loggedUserService;

    public GetOrderByIdUseCase(
        IOrderRepository orderRepository,
        ILoggedUserService loggedUserService,
        ILogger<GetOrderByIdUseCase> logger,
        IExceptionHandlerService exceptionHandler)
        : base(logger, exceptionHandler)
    {
        _orderRepository = orderRepository;
        _loggedUserService = loggedUserService;
    }

    public async Task<OrderResponse> ExecuteAsync(string id, ClaimsPrincipal user)
    {
        return await ExecuteWithExceptionHandlingAsync(async () =>
        {
            var tenantId = _loggedUserService.GetTenantId(user);
            var order = await _orderRepository.GetByIdAsync(id, tenantId);
            EnsureResourceExists(order, "Order", id);

            return new OrderResponse
            {
                Id = order.Id,
                CustomerPhoneNumber = order.CustomerPhoneNumber,
                TotalAmount = order.TotalAmount,
                PlatformFee = order.PlatformFee,
                PromotionId = order.PromotionId,
                CouponId = order.CouponId,
                Status = order.Status,
                PaymentStatus = order.PaymentStatus,
                CreatedAt = order.CreatedAt,
                UpdatedAt = order.UpdatedAt
            };
        }, "GetOrderById");
    }
}