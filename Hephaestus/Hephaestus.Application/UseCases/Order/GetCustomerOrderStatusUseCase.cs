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

public class GetCustomerOrderStatusUseCase : BaseUseCase, IGetCustomerOrderStatusUseCase
{
    private readonly IOrderRepository _orderRepository;

    public GetCustomerOrderStatusUseCase(
        IOrderRepository orderRepository,
        ILogger<GetCustomerOrderStatusUseCase> logger,
        IExceptionHandlerService exceptionHandler)
        : base(logger, exceptionHandler)
    {
        _orderRepository = orderRepository;
    }

    public async Task<IEnumerable<OrderStatusResponse>> ExecuteAsync(string customerPhoneNumber, string tenantId)
    {
        return await ExecuteWithExceptionHandlingAsync(async () =>
        {
            var orders = await _orderRepository.GetByTenantIdAsync(tenantId, customerPhoneNumber, null);
            return orders.Select(o => new OrderStatusResponse
            {
                OrderId = o.Id,
                Status = o.Status,
                CreatedAt = o.CreatedAt,
                UpdatedAt = o.UpdatedAt
            });
        }, "GetCustomerOrderStatus");
    }
}