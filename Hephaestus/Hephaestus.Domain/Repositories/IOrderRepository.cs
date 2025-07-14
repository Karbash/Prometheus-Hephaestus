using Hephaestus.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Hephaestus.Domain.Repositories;

public interface IOrderRepository
{
    Task AddAsync(Order order);
    Task<Order?> GetByIdAsync(string id, string tenantId);
    Task<IEnumerable<Order>> GetByTenantIdAsync(string tenantId, string? customerPhoneNumber, string? status);
    Task UpdateAsync(Order order);
}