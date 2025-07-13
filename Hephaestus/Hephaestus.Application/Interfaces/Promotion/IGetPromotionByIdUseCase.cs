using Hephaestus.Application.DTOs.Response;

namespace Hephaestus.Application.Interfaces.Promotion;

public interface IGetPromotionByIdUseCase
{
    Task<PromotionResponse> ExecuteAsync(string id, string tenantId);
}