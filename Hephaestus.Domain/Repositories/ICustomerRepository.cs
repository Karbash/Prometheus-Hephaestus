using Hephaestus.Application.DTOs.Response;

namespace Hephaestus.Domain.Repositories
{
    public interface ICustomerRepository
    {
        Task<PagedResult<Customer>> GetAllAsync(string? phoneNumber, string tenantId, int pageNumber = 1, int pageSize = 20);
    }
} 