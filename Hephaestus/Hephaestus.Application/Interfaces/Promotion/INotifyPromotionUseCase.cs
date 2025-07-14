using Hephaestus.Application.DTOs.Request;
using System.Security.Claims;

namespace Hephaestus.Application.Interfaces.Promotion;

public interface INotifyPromotionUseCase
{
    Task ExecuteAsync(NotifyPromotionRequest request, ClaimsPrincipal user);
}