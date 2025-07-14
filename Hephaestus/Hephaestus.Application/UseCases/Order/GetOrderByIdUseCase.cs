using Hephaestus.Application.DTOs.Response;
using Hephaestus.Domain.Entities;
using Hephaestus.Domain.Repositories;
using Hephaestus.Application.Interfaces.Order;
using Hephaestus.Application.Base;
using Hephaestus.Application.Exceptions;
using Hephaestus.Application.Services;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Hephaestus.Application.UseCases.Order;

public class GetOrderByIdUseCase : BaseUseCase, IGetOrderByIdUseCase
{
    private readonly IOrderRepository _orderRepository;

    public GetOrderByIdUseCase(
        IOrderRepository orderRepository,
        ILogger<GetOrderByIdUseCase> logger,
        IExceptionHandlerService exceptionHandler)
        : base(logger, exceptionHandler)
    {
        _orderRepository = orderRepository;
    }

    public async Task<OrderResponse> ExecuteAsync(string id, string tenantId)
    {
        return await ExecuteWithExceptionHandlingAsync(async () =>
        {
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