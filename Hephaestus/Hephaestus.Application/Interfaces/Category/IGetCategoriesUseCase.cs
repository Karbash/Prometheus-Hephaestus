using Hephaestus.Domain.DTOs.Response;
using System.Security.Claims;

namespace Hephaestus.Application.Interfaces.Category;

public interface IGetCategoriesUseCase
{
    Task<PagedResult<CategoryResponse>> ExecuteAsync(ClaimsPrincipal user, int pageNumber = 1, int pageSize = 20, string? sortBy = null, string? sortOrder = "asc");
} 