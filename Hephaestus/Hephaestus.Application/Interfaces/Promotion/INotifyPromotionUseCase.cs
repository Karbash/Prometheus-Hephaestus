using Hephaestus.Application.DTOs.Request;

namespace Hephaestus.Application.Interfaces.Promotion;

public interface INotifyPromotionUseCase
{
    Task ExecuteAsync(NotifyPromotionRequest request, string tenantId);
}