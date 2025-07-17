namespace Hephaestus.Application.Interfaces.Administration;

using Hephaestus.Domain.DTOs.Response;
using System.Collections.Generic;
using System.Threading.Tasks;

public interface IGlobalMenuItemAdminUseCase
{
    Task<PagedResult<MenuItemResponse>> ExecuteAsync(
        string? companyId = null,
        List<string>? tagIds = null,
        List<string>? categoryIds = null,
        decimal? maxPrice = null,
        bool? isAvailable = null,
        bool? promotionActiveNow = null,
        int? promotionDayOfWeek = null,
        string? promotionTime = null,
        int pageNumber = 1,
        int pageSize = 20,
        string? sortBy = null,
        string? sortOrder = "asc");
} 