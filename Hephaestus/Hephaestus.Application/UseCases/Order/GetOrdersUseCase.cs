using Hephaestus.Application.Base;
using Hephaestus.Domain.DTOs.Response;
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

    public async Task<PagedResult<OrderResponse>> ExecuteAsync(ClaimsPrincipal user, string? customerPhoneNumber, string? status, int pageNumber = 1, int pageSize = 20, string? sortBy = null, string? sortOrder = "asc")
    {
        return await ExecuteWithExceptionHandlingAsync(async () =>
        {
            var tenantId = _loggedUserService.GetTenantId(user);
            var pagedOrders = await _orderRepository.GetByTenantIdAsync(tenantId, customerPhoneNumber, status, pageNumber, pageSize, sortBy, sortOrder);
            return new PagedResult<OrderResponse>
            {
                Items = pagedOrders.Items.Select(o => new OrderResponse
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
                }).ToList(),
                TotalCount = pagedOrders.TotalCount,
                PageNumber = pagedOrders.PageNumber,
                PageSize = pagedOrders.PageSize
            };
        }, "GetOrders");
    }
}