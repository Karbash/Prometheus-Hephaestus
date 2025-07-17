using Hephaestus.Application.Base;
using Hephaestus.Application.Interfaces.Order;
using Hephaestus.Application.Services;
using Hephaestus.Domain.DTOs.Response;
using Hephaestus.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace Hephaestus.Application.UseCases.Order;

public class GlobalOrderAdminUseCase : BaseUseCase, IGlobalOrderAdminUseCase
{
    private readonly IOrderRepository _orderRepository;
    private readonly IAddressRepository _addressRepository;

    public GlobalOrderAdminUseCase(
        IOrderRepository orderRepository,
        IAddressRepository addressRepository,
        ILogger<GlobalOrderAdminUseCase> logger,
        IExceptionHandlerService exceptionHandler)
        : base(logger, exceptionHandler)
    {
        _orderRepository = orderRepository;
        _addressRepository = addressRepository;
    }

    public async Task<PagedResult<OrderResponse>> ExecuteAsync(
        string? companyId = null,
        string? customerId = null,
        string? customerPhoneNumber = null,
        string? status = null,
        string? paymentStatus = null,
        DateTime? dataInicial = null,
        DateTime? dataFinal = null,
        decimal? valorMin = null,
        decimal? valorMax = null,
        int pageNumber = 1,
        int pageSize = 20,
        string? sortBy = null,
        string? sortOrder = "asc")
    {
        return await ExecuteWithExceptionHandlingAsync(async () =>
        {
            var pagedOrders = await _orderRepository.GetAllGlobalAsync(
                companyId,
                customerId,
                customerPhoneNumber,
                status,
                paymentStatus,
                dataInicial,
                dataFinal,
                valorMin,
                valorMax,
                pageNumber,
                pageSize,
                sortBy,
                sortOrder);

            var orderResponses = new List<OrderResponse>();
            foreach (var o in pagedOrders.Items)
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
                TotalCount = pagedOrders.TotalCount,
                PageNumber = pagedOrders.PageNumber,
                PageSize = pagedOrders.PageSize
            };
        }, "GlobalOrderAdmin");
    }
} 