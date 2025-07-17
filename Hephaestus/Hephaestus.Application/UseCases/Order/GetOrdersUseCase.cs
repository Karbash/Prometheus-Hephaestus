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
    private readonly IAddressRepository _addressRepository;

    public GetOrdersUseCase(
        IOrderRepository orderRepository,
        ILoggedUserService loggedUserService,
        ILogger<GetOrdersUseCase> logger,
        IExceptionHandlerService exceptionHandler,
        IAddressRepository addressRepository)
        : base(logger, exceptionHandler)
    {
        _orderRepository = orderRepository;
        _loggedUserService = loggedUserService;
        _addressRepository = addressRepository;
    }

    public async Task<PagedResult<OrderResponse>> ExecuteAsync(ClaimsPrincipal user, string? customerPhoneNumber, string? status, int pageNumber = 1, int pageSize = 20, string? sortBy = null, string? sortOrder = "asc")
    {
        return await ExecuteWithExceptionHandlingAsync(async () =>
        {
            var tenantId = _loggedUserService.GetTenantId(user);
            var pagedOrders = await _orderRepository.GetByTenantIdAsync(tenantId, customerPhoneNumber, status, pageNumber, pageSize, sortBy, sortOrder);
            // Filtro extra: se status n�o for informado, s� retorna pedidos n�o pendentes
            var filteredItems = string.IsNullOrEmpty(status)
                ? pagedOrders.Items.Where(o => o.Status != Hephaestus.Domain.Enum.OrderStatus.Pending).ToList()
                : pagedOrders.Items.ToList();
            var orderResponses = new List<OrderResponse>();
            foreach (var o in filteredItems)
            {
                var address = (await _addressRepository.GetByEntityAsync(o.Id, "Order")).FirstOrDefault();
                orderResponses.Add(new OrderResponse
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
                    UpdatedAt = o.UpdatedAt,
                    Items = o.OrderItems?.Select(oi => new OrderItemResponse
                    {
                        Id = oi.Id,
                        MenuItemId = oi.MenuItemId,
                        Quantity = oi.Quantity,
                        UnitPrice = oi.UnitPrice,
                        Notes = oi.Notes,
                        AdditionalIds = oi.OrderItemAdditionals?.Select(a => a.AdditionalId).ToList() ?? new List<string>(),
                        TagIds = oi.OrderItemTags?.Select(t => t.TagId).ToList() ?? new List<string>(),
                        Customizations = oi.Customizations?.Select(c => new CustomizationResponse
                        {
                            Type = c.Type,
                            Value = c.Value
                        }).ToList()
                    }).ToList() ?? new List<OrderItemResponse>()
                });
            }
            return new PagedResult<OrderResponse>
            {
                Items = orderResponses,
                TotalCount = filteredItems.Count,
                PageNumber = pagedOrders.PageNumber,
                PageSize = pagedOrders.PageSize
            };
        }, "GetOrders");
    }
}
