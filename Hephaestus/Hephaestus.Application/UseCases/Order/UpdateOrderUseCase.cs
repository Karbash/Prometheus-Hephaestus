using FluentValidation;
using Hephaestus.Application.Base;
using Hephaestus.Application.Interfaces.Order;
using Hephaestus.Application.Services;
using Hephaestus.Domain.DTOs.Request;
using Hephaestus.Domain.Entities;
using Hephaestus.Domain.Enum;
using Hephaestus.Domain.Repositories;
using Hephaestus.Domain.Services;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace Hephaestus.Application.UseCases.Order;

public class UpdateOrderUseCase : BaseUseCase, IUpdateOrderUseCase
{
    private readonly IOrderRepository _orderRepository;
    private readonly IMenuItemRepository _menuItemRepository;
    private readonly IValidator<UpdateOrderRequest> _validator;
    private readonly ILoggedUserService _loggedUserService;
    private readonly ICompanyRepository _companyRepository;
    private readonly IAddressRepository _addressRepository;
    // Remover o uso do DbContext

    public UpdateOrderUseCase(
        IOrderRepository orderRepository,
        IMenuItemRepository menuItemRepository,
        IValidator<UpdateOrderRequest> validator,
        ILoggedUserService loggedUserService,
        ICompanyRepository companyRepository,
        IAddressRepository addressRepository,
        ILogger<UpdateOrderUseCase> logger,
        IExceptionHandlerService exceptionHandler)
        : base(logger, exceptionHandler)
    {
        _orderRepository = orderRepository;
        _menuItemRepository = menuItemRepository;
        _validator = validator;
        _loggedUserService = loggedUserService;
        _companyRepository = companyRepository;
        _addressRepository = addressRepository;
    }

    public async Task ExecuteAsync(UpdateOrderRequest request, ClaimsPrincipal user)
    {
        await ExecuteWithExceptionHandlingAsync(async () =>
        {
            await ValidateAsync(_validator, request);
            var tenantId = _loggedUserService.GetTenantId(user);

            // Buscar a Order com seus itens via reposit�rio
            var order = await _orderRepository.GetByIdWithItemsAsync(request.OrderId, tenantId);
            EnsureResourceExists(order, "Order", request.OrderId);

            // Atualiza campos simples
            order.CustomerPhoneNumber = request.CustomerPhoneNumber;
            order.PromotionId = request.PromotionId;
            order.CouponId = request.CouponId;
            if (request.Status.HasValue)
                order.Status = request.Status.Value;
            if (request.PaymentStatus.HasValue)
                order.PaymentStatus = request.PaymentStatus.Value;
            order.UpdatedAt = DateTime.UtcNow;

            // Merge seguro dos OrderItems
            var updatedItems = new List<OrderItem>();
            decimal totalAmount = 0;
            foreach (var item in request.Items)
            {
                var menuItem = await _menuItemRepository.GetByIdAsync(item.MenuItemId, tenantId);
                EnsureResourceExists(menuItem, "MenuItem", item.MenuItemId);
                var existingItem = order.OrderItems.FirstOrDefault(oi => oi.Id == item.Id);
                if (existingItem != null)
                {
                    // Atualiza item existente
                    existingItem.MenuItemId = item.MenuItemId;
                    existingItem.Quantity = item.Quantity;
                    existingItem.UnitPrice = menuItem.Price;
                    existingItem.Notes = item.Notes ?? string.Empty;
                    existingItem.Customizations = item.Customizations?
                        .Select(c => new Customization
                        {
                            Type = c.Type,
                            Value = c.Value
                        }).ToList() ?? new List<Customization>();
                    // Atualiza adicionais
                    existingItem.OrderItemAdditionals = item.AdditionalIds?.Select(aid => new OrderItemAdditional
                    {
                        OrderItemId = existingItem.Id,
                        AdditionalId = aid,
                        TenantId = tenantId,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    }).ToList() ?? new List<OrderItemAdditional>();
                    // Atualiza tags
                    existingItem.OrderItemTags = item.TagIds?.Select(tid => new OrderItemTag
                    {
                        OrderItemId = existingItem.Id,
                        TagId = tid,
                        TenantId = tenantId,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    }).ToList() ?? new List<OrderItemTag>();
                    updatedItems.Add(existingItem);
                }
                else
                {
                    // Novo item
                    var newOrderItemId = Guid.NewGuid().ToString();
                    updatedItems.Add(new OrderItem
                    {
                        Id = newOrderItemId,
                        TenantId = tenantId,
                        OrderId = order.Id,
                        MenuItemId = item.MenuItemId,
                        Quantity = item.Quantity,
                        UnitPrice = menuItem.Price,
                        Notes = item.Notes ?? string.Empty,
                        Customizations = item.Customizations?
                            .Select(c => new Customization
                            {
                                Type = c.Type,
                                Value = c.Value
                            }).ToList() ?? new List<Customization>(),
                        OrderItemAdditionals = item.AdditionalIds?.Select(aid => new OrderItemAdditional
                        {
                            OrderItemId = newOrderItemId,
                            AdditionalId = aid,
                            TenantId = tenantId,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        }).ToList() ?? new List<OrderItemAdditional>(),
                        OrderItemTags = item.TagIds?.Select(tid => new OrderItemTag
                        {
                            OrderItemId = newOrderItemId,
                            TagId = tid,
                            TenantId = tenantId,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        }).ToList() ?? new List<OrderItemTag>()
                    });
                }
                totalAmount += item.Quantity * menuItem.Price;
            }
            // Remove itens que n�o est�o mais presentes
            var itemsToRemove = order.OrderItems.Where(oi => !updatedItems.Any(ui => ui.Id == oi.Id)).ToList();
            foreach (var item in itemsToRemove)
            {
                order.OrderItems.Remove(item);
            }
            // Atualiza a cole��o
            order.OrderItems = updatedItems;
            order.TotalAmount = totalAmount;

            // Atualizar endereço se enviado
            if (request.Address != null)
            {
                var addresses = await _addressRepository.GetByEntityAsync(order.Id, "Order");
                var address = addresses.FirstOrDefault();
                if (address != null)
                {
                    address.Street = request.Address.Street;
                    address.Number = request.Address.Number;
                    address.Complement = request.Address.Complement;
                    address.Neighborhood = request.Address.Neighborhood;
                    address.City = request.Address.City;
                    address.State = request.Address.State;
                    address.ZipCode = request.Address.ZipCode;
                    address.Reference = request.Address.Reference;
                    address.Notes = request.Address.Notes;
                    address.Latitude = request.Address.Latitude ?? 0;
                    address.Longitude = request.Address.Longitude ?? 0;
                    address.UpdatedAt = DateTime.UtcNow;
                    await _addressRepository.UpdateAsync(address);
                }
            }

            // Buscar a empresa e recalcular a taxa de plataforma
            var company = await _companyRepository.GetByIdAsync(tenantId);
            EnsureResourceExists(company, "Company", tenantId);
            order.PlatformFee = company.FeeType == FeeType.Percentage ? totalAmount * (company.FeeValue / 100) : company.FeeValue;

            // Persistir altera��es via reposit�rio
            await _orderRepository.UpdateAsync(order);
        }, "UpdateOrder");
    }

    public async Task ExecutePartialAsync(UpdateOrderRequest request, ClaimsPrincipal user)
    {
        await ExecuteWithExceptionHandlingAsync(async () =>
        {
            var tenantId = _loggedUserService.GetTenantId(user);
            var order = await _orderRepository.GetByIdWithItemsAsync(request.OrderId, tenantId);
            EnsureResourceExists(order, "Order", request.OrderId);

            if (request.CustomerPhoneNumber != null)
                order.CustomerPhoneNumber = request.CustomerPhoneNumber;
            if (request.PromotionId != null)
                order.PromotionId = request.PromotionId;
            if (request.CouponId != null)
                order.CouponId = request.CouponId;
            if (request.Status.HasValue)
                order.Status = request.Status.Value;
            if (request.PaymentStatus.HasValue)
                order.PaymentStatus = request.PaymentStatus.Value;
            if (request.Items != null)
            {
                var updatedItems = new List<OrderItem>();
                decimal totalAmount = 0;
                foreach (var item in request.Items)
                {
                    var menuItem = await _menuItemRepository.GetByIdAsync(item.MenuItemId, tenantId);
                    EnsureResourceExists(menuItem, "MenuItem", item.MenuItemId);
                    var existingItem = order.OrderItems.FirstOrDefault(oi => oi.Id == item.Id);
                    if (existingItem != null)
                    {
                        existingItem.MenuItemId = item.MenuItemId;
                        existingItem.Quantity = item.Quantity;
                        existingItem.UnitPrice = menuItem.Price;
                        existingItem.Notes = item.Notes ?? string.Empty;
                        existingItem.Customizations = item.Customizations?
                            .Select(c => new Customization
                            {
                                Type = c.Type,
                                Value = c.Value
                            }).ToList() ?? new List<Customization>();
                        // Removido: OrderItemAdditionals = item.AdditionalIds?.Select(...)
                        // Agora, OrderItemAdditionals deve ser manipulado via entidade de ligação normalizada
                        // Se necessário, adicione lógica para buscar additionals normalizados aqui
                        updatedItems.Add(existingItem);
                    }
                    else
                    {
                        updatedItems.Add(new OrderItem
                        {
                            Id = Guid.NewGuid().ToString(),
                            TenantId = tenantId,
                            OrderId = order.Id,
                            MenuItemId = item.MenuItemId,
                            Quantity = item.Quantity,
                            UnitPrice = menuItem.Price,
                            Notes = item.Notes ?? string.Empty,
                            Customizations = item.Customizations?
                                .Select(c => new Customization
                                {
                                    Type = c.Type,
                                    Value = c.Value
                                }).ToList() ?? new List<Customization>(),
                            // OrderItemAdditionals removido: agora deve ser manipulado via entidade de ligação normalizada
                        });
                    }
                    totalAmount += item.Quantity * menuItem.Price;
                }
                var itemsToRemove = order.OrderItems.Where(oi => !updatedItems.Any(ui => ui.Id == oi.Id)).ToList();
                foreach (var item in itemsToRemove)
                {
                    order.OrderItems.Remove(item);
                }
                order.OrderItems = updatedItems;
                order.TotalAmount = totalAmount;
                // Recalcula a taxa de plataforma
                var company = await _companyRepository.GetByIdAsync(tenantId);
                EnsureResourceExists(company, "Company", tenantId);
                order.PlatformFee = company.FeeType == FeeType.Percentage ? totalAmount * (company.FeeValue / 100) : company.FeeValue;
            }
            order.UpdatedAt = DateTime.UtcNow;
            await _orderRepository.UpdateAsync(order);
        }, "PatchOrder");
    }
}
