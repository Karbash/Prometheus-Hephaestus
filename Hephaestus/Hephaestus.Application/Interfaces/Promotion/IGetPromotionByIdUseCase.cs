using Hephaestus.Domain.DTOs.Response;
using System.Security.Claims;

namespace Hephaestus.Application.Interfaces.Promotion;

public interface IGetPromotionByIdUseCase
{
    Task<PromotionResponse> ExecuteAsync(string id, ClaimsPrincipal user);
}