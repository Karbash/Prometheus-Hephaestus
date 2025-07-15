using Hephaestus.Application.Base;
using Hephaestus.Application.DTOs.Response;
using Hephaestus.Application.Interfaces.Order;
using Hephaestus.Application.Services;
using Hephaestus.Domain.Repositories;
using Hephaestus.Domain.Services;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace Hephaestus.Application.UseCases.Order;

public class GetCustomerOrderStatusUseCase : BaseUseCase, IGetCustomerOrderStatusUseCase
{
    private readonly IOrderRepository _orderRepository;
    private readonly ILoggedUserService _loggedUserService;

    public GetCustomerOrderStatusUseCase(
        IOrderRepository orderRepository,
        ILoggedUserService loggedUserService,
        ILogger<GetCustomerOrderStatusUseCase> logger,
        IExceptionHandlerService exceptionHandler)
        : base(logger, exceptionHandler)
    {
        _orderRepository = orderRepository;
        _loggedUserService = loggedUserService;
    }

    public async Task<IEnumerable<OrderStatusResponse>> ExecuteAsync(string customerPhoneNumber, ClaimsPrincipal user)
    {
        return await ExecuteWithExceptionHandlingAsync(async () =>
        {
            var tenantId = _loggedUserService.GetTenantId(user);
            var orders = await _orderRepository.GetByTenantIdAsync(tenantId, customerPhoneNumber, null);
            return orders.Items.Select(o => new OrderStatusResponse
            {
                OrderId = o.Id,
                Status = o.Status,
                CreatedAt = o.CreatedAt,
                UpdatedAt = o.UpdatedAt
            });
        }, "GetCustomerOrderStatus");
    }
}