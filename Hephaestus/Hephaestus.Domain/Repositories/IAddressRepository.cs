using Hephaestus.Domain.Entities;

namespace Hephaestus.Domain.Repositories;

public interface IAddressRepository
{
    Task AddAsync(Address address);
    Task UpdateAsync(Address address);
    Task<Address?> GetByIdAsync(string id);
    Task<IEnumerable<Address>> GetByEntityAsync(string entityId, string entityType);
    Task DeleteAsync(string id);
} 