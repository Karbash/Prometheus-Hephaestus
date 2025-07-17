using Hephaestus.Application.Base;
using Hephaestus.Domain.DTOs.Response;
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
    private readonly IAddressRepository _addressRepository;

    public GetOrderByIdUseCase(
        IOrderRepository orderRepository,
        ILoggedUserService loggedUserService,
        ILogger<GetOrderByIdUseCase> logger,
        IExceptionHandlerService exceptionHandler,
        IAddressRepository addressRepository)
        : base(logger, exceptionHandler)
    {
        _orderRepository = orderRepository;
        _loggedUserService = loggedUserService;
        _addressRepository = addressRepository;
    }

    public async Task<OrderResponse> ExecuteAsync(string id, ClaimsPrincipal user)
    {
        return await ExecuteWithExceptionHandlingAsync(async () =>
        {
            var tenantId = _loggedUserService.GetTenantId(user);
            var order = await _orderRepository.GetByIdAsync(id, tenantId);
            EnsureResourceExists(order, "Order", id);
            var address = (await _addressRepository.GetByEntityAsync(order.Id, "Order")).FirstOrDefault();
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
                UpdatedAt = order.UpdatedAt,
                // Address removido, pois agora é tratado via entidade normalizada
                Items = order.OrderItems?.Select(oi => new OrderItemResponse
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
            };
        }, "GetOrderById");
    }
}
