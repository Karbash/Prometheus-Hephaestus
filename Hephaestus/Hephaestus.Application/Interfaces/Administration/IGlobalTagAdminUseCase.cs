namespace Hephaestus.Application.Interfaces.Administration;

using Hephaestus.Domain.DTOs.Response;
using System.Threading.Tasks;

public interface IGlobalTagAdminUseCase
{
    Task<PagedResult<TagResponse>> ExecuteAsync(
        string? companyId = null,
        string? name = null,
        int pageNumber = 1,
        int pageSize = 20,
        string? sortBy = null,
        string? sortOrder = "asc");
} 