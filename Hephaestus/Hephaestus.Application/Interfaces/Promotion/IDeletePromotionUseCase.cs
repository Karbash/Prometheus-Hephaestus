using System.Security.Claims;

namespace Hephaestus.Application.Interfaces.Promotion;

public interface IDeletePromotionUseCase
{
    Task ExecuteAsync(string id, ClaimsPrincipal user);
}