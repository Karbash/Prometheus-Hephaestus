using Hephaestus.Domain.DTOs.Response;
using System.Security.Claims;

namespace Hephaestus.Application.Interfaces.Promotion;

public interface IGetPromotionsUseCase
{
    Task<PagedResult<PromotionResponse>> ExecuteAsync(System.Security.Claims.ClaimsPrincipal user, bool? isActive, int pageNumber = 1, int pageSize = 20, string? sortBy = null, string? sortOrder = "asc");
}