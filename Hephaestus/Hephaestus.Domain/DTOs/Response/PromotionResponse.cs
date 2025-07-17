using Hephaestus.Domain.Enum;

namespace Hephaestus.Domain.DTOs.Response;

public class PromotionResponse
{
    public string Id { get; set; } = string.Empty;
    public string CompanyId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DiscountType DiscountType { get; set; }
    public decimal DiscountValue { get; set; }
    public string? MenuItemId { get; set; }
    public decimal? MinOrderValue { get; set; }
    public int? MaxTotalUses { get; set; }
    public int? MaxUsesPerCustomer { get; set; }
    public string DaysOfWeek { get; set; } = string.Empty;
    public string Hours { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsActive { get; set; }
    public string? ImageUrl { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
