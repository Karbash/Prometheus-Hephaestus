using Hephaestus.Application.Base;
using Hephaestus.Application.Interfaces.Order;
using Hephaestus.Application.Services;
using Hephaestus.Domain.DTOs.Response;
using Hephaestus.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace Hephaestus.Application.UseCases.Order;

public class GlobalOrderItemAdminUseCase : BaseUseCase, IGlobalOrderItemAdminUseCase
{
    private readonly IOrderRepository _orderRepository;

    public GlobalOrderItemAdminUseCase(
        IOrderRepository orderRepository,
        ILogger<GlobalOrderItemAdminUseCase> logger,
        IExceptionHandlerService exceptionHandler)
        : base(logger, exceptionHandler)
    {
        _orderRepository = orderRepository;
    }

    public async Task<PagedResult<OrderItemResponse>> ExecuteAsync(
        string? orderId = null,
        string? companyId = null,
        string? customerId = null,
        string? menuItemId = null,
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
            var pagedItems = await _orderRepository.GetAllOrderItemsGlobalAsync(
                orderId,
                companyId,
                customerId,
                menuItemId,
                dataInicial,
                dataFinal,
                valorMin,
                valorMax,
                pageNumber,
                pageSize,
                sortBy,
                sortOrder);

            var itemResponses = pagedItems.Items.Select(oi => new OrderItemResponse
            {
                Id = oi.Id,
                MenuItemId = oi.MenuItemId,
                Quantity = oi.Quantity,
                UnitPrice = oi.UnitPrice,
                Notes = oi.Notes,
                Customizations = oi.Customizations?.Select(c => new CustomizationResponse
                {
                    Type = c.Type,
                    Value = c.Value
                }).ToList(),
                AdditionalIds = oi.OrderItemAdditionals?.Select(a => a.AdditionalId).ToList(),
                TagIds = oi.OrderItemTags?.Select(t => t.TagId).ToList()
            }).ToList();

            return new PagedResult<OrderItemResponse>
            {
                Items = itemResponses,
                TotalCount = pagedItems.TotalCount,
                PageNumber = pagedItems.PageNumber,
                PageSize = pagedItems.PageSize
            };
        }, "GlobalOrderItemAdmin");
    }
} 