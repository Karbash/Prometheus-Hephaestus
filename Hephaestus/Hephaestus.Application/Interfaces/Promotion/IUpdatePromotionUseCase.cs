using Hephaestus.Domain.DTOs.Request;
using System.Security.Claims;

namespace Hephaestus.Application.Interfaces.Promotion;

public interface IUpdatePromotionUseCase
{
    Task ExecuteAsync(string id, UpdatePromotionRequest request, ClaimsPrincipal user);
}
