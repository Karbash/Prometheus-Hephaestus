using Hephaestus.Domain.Entities;
using Hephaestus.Domain.DTOs.Response;

namespace Hephaestus.Domain.Repositories;

public interface IAdditionalRepository
{
    Task AddAsync(Additional additional);
    Task<PagedResult<Additional>> GetByTenantIdAsync(string tenantId, int pageNumber = 1, int pageSize = 20, string? sortBy = null, string? sortOrder = "asc");
    Task<Additional?> GetByIdAsync(string id, string tenantId);
    Task UpdateAsync(Additional additional);
    Task DeleteAsync(string id, string tenantId);
    Task<PagedResult<Additional>> GetAllGlobalAsync(
        string? tenantId = null,
        string? name = null,
        bool? isAvailable = null,
        decimal? precoMin = null,
        decimal? precoMax = null,
        DateTime? dataInicial = null,
        DateTime? dataFinal = null,
        int pageNumber = 1,
        int pageSize = 20,
        string? sortBy = null,
        string? sortOrder = "asc");
}
