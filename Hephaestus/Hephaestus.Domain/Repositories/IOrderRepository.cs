using Hephaestus.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;
using Hephaestus.Domain.DTOs.Response;
using System;

namespace Hephaestus.Domain.Repositories;

public interface IOrderRepository
{
    Task AddAsync(Order order);
    Task<Order?> GetByIdAsync(string id, string tenantId);
    Task<Order?> GetByIdWithItemsAsync(string id, string tenantId);
    Task<PagedResult<Order>> GetByTenantIdAsync(string tenantId, string? customerPhoneNumber, string? status, int pageNumber = 1, int pageSize = 20, string? sortBy = null, string? sortOrder = "asc");
    Task UpdateAsync(Order order);
    Task<List<Order>> GetPendingOrdersOlderThanAsync(DateTime cutoffUtc);
    Task DeleteAsync(string id, string tenantId);
    Task<PagedResult<Order>> GetAllGlobalAsync(
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
        string? sortOrder = "asc");
    Task<PagedResult<OrderItem>> GetAllOrderItemsGlobalAsync(
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
        string? sortOrder = "asc");
}
