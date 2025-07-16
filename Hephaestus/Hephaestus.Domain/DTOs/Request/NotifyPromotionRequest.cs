namespace Hephaestus.Domain.DTOs.Request;

public class NotifyPromotionRequest
{
    public string PromotionId { get; set; } = string.Empty;
    public string MessageTemplate { get; set; } = string.Empty;
}
