using Hephaestus.Application.Base;
using Hephaestus.Application.Interfaces.Order;
using Hephaestus.Domain.DTOs.Request;
using Hephaestus.Domain.Entities;
using Hephaestus.Domain.Repositories;
using Hephaestus.Domain.Services;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hephaestus.Application.Services;
using Hephaestus.Domain.Enum;
using Hephaestus.Application.Exceptions;

namespace Hephaestus.Application.UseCases.Order
{
    public class PatchOrderUseCase : BaseUseCase, IPatchOrderUseCase
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IMenuItemRepository _menuItemRepository;
        private readonly ICompanyRepository _companyRepository;
        private readonly ILoggedUserService _loggedUserService;

        public PatchOrderUseCase(
            IOrderRepository orderRepository,
            IMenuItemRepository menuItemRepository,
            ICompanyRepository companyRepository,
            ILoggedUserService loggedUserService,
            ILogger<PatchOrderUseCase> logger,
            IExceptionHandlerService exceptionHandler)
            : base(logger, exceptionHandler)
        {
            _orderRepository = orderRepository;
            _menuItemRepository = menuItemRepository;
            _companyRepository = companyRepository;
            _loggedUserService = loggedUserService;
        }

        public async Task ExecuteAsync(UpdateOrderRequest request, ClaimsPrincipal user)
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
                // Regras de transição de status
                if (request.Status.HasValue && request.Status.Value != order.Status)
                {
                    var currentStatus = order.Status;
                    var newStatus = request.Status.Value;
                    var paymentStatus = order.PaymentStatus;

                    // Não permitir cancelar pedido já pago
                    if (newStatus == OrderStatus.Cancelled && paymentStatus == PaymentStatus.Paid)
                        throw new BusinessRuleException("Não é possível cancelar um pedido já pago.", "ORDER_CANCEL_PAID");

                    // Não permitir finalizar pedido que não está em produção
                    if (newStatus == OrderStatus.Completed && currentStatus != OrderStatus.InProduction)
                        throw new BusinessRuleException("Só é possível finalizar pedidos que estão em produção.", "ORDER_FINALIZE_NOT_IN_PRODUCTION");

                    // Não permitir pular etapas (ex: Pending → Finalized)
                    if (currentStatus == OrderStatus.Pending && newStatus == OrderStatus.Completed)
                        throw new BusinessRuleException("Não é possível finalizar um pedido pendente.", "ORDER_FINALIZE_PENDING");
                    if (currentStatus == OrderStatus.Pending && newStatus == OrderStatus.InProduction)
                        throw new BusinessRuleException("Não é possível mover pedido pendente direto para produção.", "ORDER_PRODUCTION_PENDING");
                    if (currentStatus == OrderStatus.InProduction && newStatus == OrderStatus.Completed)
                        throw new BusinessRuleException("Pedido deve estar em produção antes de ser finalizado.", "ORDER_FINALIZE_NOT_IN_PRODUCTION");

                    // Permitir apenas transições válidas
                    var validTransitions = new Dictionary<OrderStatus, List<OrderStatus>>
                    {
                        { OrderStatus.Pending, new List<OrderStatus> { OrderStatus.InProduction, OrderStatus.Cancelled } },
                        { OrderStatus.InProduction, new List<OrderStatus> { OrderStatus.Completed, OrderStatus.Cancelled } },
                        { OrderStatus.Completed, new List<OrderStatus>() },
                        { OrderStatus.Cancelled, new List<OrderStatus>() }
                    };
                    if (!validTransitions[currentStatus].Contains(newStatus))
                        throw new BusinessRuleException($"Transição de status inválida: {currentStatus} → {newStatus}.", "ORDER_INVALID_STATUS_TRANSITION");

                    order.Status = newStatus;
                }
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
                            existingItem.Tags = string.Join(",", item.Tags ?? new List<string>());
                            existingItem.AdditionalIds = string.Join(",", item.AdditionalIds ?? new List<string>());
                            existingItem.Customizations = item.Customizations?
                                .Select(c => new Customization
                                {
                                    Type = c.Type,
                                    Value = c.Value
                                }).ToList() ?? new List<Customization>();
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
                                Tags = string.Join(",", item.Tags ?? new List<string>()),
                                AdditionalIds = string.Join(",", item.AdditionalIds ?? new List<string>()),
                                Customizations = item.Customizations?
                                    .Select(c => new Customization
                                    {
                                        Type = c.Type,
                                        Value = c.Value
                                    }).ToList() ?? new List<Customization>()
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
                }
                // Sempre recalcula a taxa de plataforma
                var company = await _companyRepository.GetByIdAsync(tenantId);
                EnsureResourceExists(company, "Company", tenantId);
                order.PlatformFee = company.FeeType == FeeType.Percentage ? order.TotalAmount * (company.FeeValue / 100) : company.FeeValue;
                order.UpdatedAt = DateTime.UtcNow;
                await _orderRepository.UpdateAsync(order);
            }, "PatchOrder");
        }
    }
} 
