using Hephaestus.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;
using Hephaestus.Domain.DTOs.Response;

namespace Hephaestus.Domain.Repositories;

public interface IOrderRepository
{
    Task AddAsync(Order order);
    Task<Order?> GetByIdAsync(string id, string tenantId);
    Task<Order?> GetByIdWithItemsAsync(string id, string tenantId);
    Task<PagedResult<Order>> GetByTenantIdAsync(string tenantId, string? customerPhoneNumber, string? status, int pageNumber = 1, int pageSize = 20, string? sortBy = null, string? sortOrder = "asc");
    Task UpdateAsync(Order order);
}