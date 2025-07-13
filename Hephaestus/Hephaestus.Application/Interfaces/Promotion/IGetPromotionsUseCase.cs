using Hephaestus.Application.DTOs.Response;

namespace Hephaestus.Application.Interfaces.Promotion;

public interface IGetPromotionsUseCase
{
    Task<IEnumerable<PromotionResponse>> ExecuteAsync(string tenantId, bool? isActive);
}