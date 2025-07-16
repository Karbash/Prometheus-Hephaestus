using Hephaestus.Domain.DTOs.Request;
using System.Security.Claims;

namespace Hephaestus.Application.Interfaces.Promotion;

public interface ICreatePromotionUseCase
{
    Task<string> ExecuteAsync(CreatePromotionRequest request, ClaimsPrincipal user);
}
