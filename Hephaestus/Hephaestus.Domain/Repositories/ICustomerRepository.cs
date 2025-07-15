using Hephaestus.Domain.DTOs.Response;
using Hephaestus.Domain.Entities;

namespace Hephaestus.Domain.Repositories
{
    public interface ICustomerRepository
    {
        Task<PagedResult<Customer>> GetAllAsync(string? phoneNumber, string tenantId, int pageNumber = 1, int pageSize = 20, string? sortBy = null, string? sortOrder = "asc");
        Task<Customer?> GetByIdAsync(string id, string tenantId);
        Task<Customer?> GetByPhoneNumberAsync(string phoneNumber, string tenantId);
        Task AddAsync(Customer customer);
        Task UpdateAsync(Customer customer);
        Task DeleteAsync(string id, string tenantId);
    }
} 