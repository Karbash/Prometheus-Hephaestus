using Hephaestus.Domain.Entities;

namespace Hephaestus.Domain.Repositories;

public interface ICustomerRepository
{
    Task<Customer?> GetByPhoneNumberAsync(string phoneNumber, string tenantId);
    Task<IEnumerable<Customer>> GetAllAsync(string? phoneNumber, string tenantId);
    Task<Customer?> GetByIdAsync(string id, string tenantId);
    Task AddAsync(Customer customer);
    Task UpdateAsync(Customer customer);
}