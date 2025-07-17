using Hephaestus.Domain.DTOs.Response;
using System.Threading.Tasks;

namespace Hephaestus.Application.Interfaces.Promotion;

public interface IGetAllPromotionsAdminUseCase
{
    Task<PagedResult<PromotionResponse>> ExecuteAsync(bool? isActive = null, string? companyId = null, int pageNumber = 1, int pageSize = 20, string? sortBy = null, string? sortOrder = "asc");
} 