using Hephaestus.Application.DTOs.Request;

namespace Hephaestus.Application.Interfaces.Promotion;

public interface IUpdatePromotionUseCase
{
    Task ExecuteAsync(string id, UpdatePromotionRequest request, string tenantId);
}