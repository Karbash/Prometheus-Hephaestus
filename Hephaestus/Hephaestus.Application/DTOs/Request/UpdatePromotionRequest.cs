namespace Hephaestus.Application.DTOs.Request;

public class UpdatePromotionRequest
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string DiscountType { get; set; } = string.Empty;
    public decimal DiscountValue { get; set; }
    public string? MenuItemId { get; set; }
    public decimal? MinOrderValue { get; set; }
    public int? MaxUsagePerCustomer { get; set; }
    public int? MaxTotalUses { get; set; }
    public List<string>? ApplicableToTags { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsActive { get; set; }
    public string? ImageUrl { get; set; }
}