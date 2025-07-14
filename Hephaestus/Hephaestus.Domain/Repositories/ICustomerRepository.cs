using Hephaestus.Domain.Entities;
using Hephaestus.Application.DTOs.Response;

namespace Hephaestus.Domain.Repositories;

public interface ICustomerRepository
{
    Task<Customer?> GetByPhoneNumberAsync(string phoneNumber, string tenantId);
    Task<PagedResult<Customer>> GetAllAsync(string? phoneNumber, string tenantId, int pageNumber = 1, int pageSize = 20);
    Task<Customer?> GetByIdAsync(string id, string tenantId);
    Task AddAsync(Customer customer);
    Task UpdateAsync(Customer customer);
}