using Hephaestus.Application.DTOs.Response;
using System.Security.Claims;

namespace Hephaestus.Application.Interfaces.Promotion;

public interface IGetPromotionsUseCase
{
    Task<IEnumerable<PromotionResponse>> ExecuteAsync(ClaimsPrincipal user, bool? isActive);
}