using Hephaestus.Application.Interfaces.Promotion;
using Hephaestus.Domain.Repositories;

namespace Hephaestus.Application.UseCases.Promotion;

public class DeletePromotionUseCase : IDeletePromotionUseCase
{
    private readonly IPromotionRepository _promotionRepository;

    public DeletePromotionUseCase(IPromotionRepository promotionRepository)
    {
        _promotionRepository = promotionRepository;
    }

    public async Task ExecuteAsync(string id, string tenantId)
    {
        await _promotionRepository.DeleteAsync(id, tenantId);
    }
}