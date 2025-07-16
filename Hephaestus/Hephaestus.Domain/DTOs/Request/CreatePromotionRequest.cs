using Hephaestus.Domain.Enum;

namespace Hephaestus.Domain.DTOs.Request;

public class CreatePromotionRequest
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DiscountType DiscountType { get; set; }
    public decimal DiscountValue { get; set; }
    public string? MenuItemId { get; set; }
    public decimal? MinOrderValue { get; set; }
    public int? MaxTotalUses { get; set; }
    public int? MaxUsesPerCustomer { get; set; }
    public List<string>? ApplicableToTags { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsActive { get; set; } = true;
    public string? ImageUrl { get; set; }
}
