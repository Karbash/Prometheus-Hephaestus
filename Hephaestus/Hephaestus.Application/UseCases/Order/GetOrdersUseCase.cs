using Hephaestus.Application.DTOs.Response;
using Hephaestus.Domain.Repositories;
using Hephaestus.Application.Interfaces.Order;
using Hephaestus.Application.Base;
using Hephaestus.Application.Services;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hephaestus.Application.UseCases.Order;

public class GetOrdersUseCase : BaseUseCase, IGetOrdersUseCase
{
    private readonly IOrderRepository _orderRepository;

    public GetOrdersUseCase(
        IOrderRepository orderRepository,
        ILogger<GetOrdersUseCase> logger,
        IExceptionHandlerService exceptionHandler)
        : base(logger, exceptionHandler)
    {
        _orderRepository = orderRepository;
    }

    public async Task<IEnumerable<OrderResponse>> ExecuteAsync(string tenantId, string? customerPhoneNumber, string? status)
    {
        return await ExecuteWithExceptionHandlingAsync(async () =>
        {
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