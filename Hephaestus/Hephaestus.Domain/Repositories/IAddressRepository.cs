using Hephaestus.Domain.Entities;
using Hephaestus.Domain.DTOs.Response;

namespace Hephaestus.Domain.Repositories;

public interface IAddressRepository
{
    Task AddAsync(Address address);
    Task UpdateAsync(Address address);
    Task<Address?> GetByIdAsync(string id);
    Task<IEnumerable<Address>> GetByEntityAsync(string entityId, string entityType);
    Task DeleteAsync(string id);
    Task<PagedResult<Address>> GetAllGlobalAsync(
        string? entityId = null,
        string? entityType = null,
        string? city = null,
        string? state = null,
        string? type = null,
        DateTime? dataInicial = null,
        DateTime? dataFinal = null,
        int pageNumber = 1,
        int pageSize = 20,
        string? sortBy = null,
        string? sortOrder = "asc");
} 