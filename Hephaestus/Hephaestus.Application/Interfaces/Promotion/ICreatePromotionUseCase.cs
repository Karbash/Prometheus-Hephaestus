using Hephaestus.Application.DTOs.Request;

namespace Hephaestus.Application.Interfaces.Promotion;

public interface ICreatePromotionUseCase
{
    Task<string> ExecuteAsync(CreatePromotionRequest request, string tenantId);
}