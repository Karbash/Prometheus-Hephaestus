using Hephaestus.Domain.Enum;

namespace Hephaestus.Domain.Entities;

public class Promotion
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string TenantId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DiscountType DiscountType { get; set; }
    public decimal DiscountValue { get; set; }
    public string? MenuItemId { get; set; }
    public decimal? MinOrderValue { get; set; }
    public int? MaxUsesPerCustomer { get; set; }
    public int? MaxTotalUses { get; set; }
    public List<string> ApplicableTags { get; set; } = new List<string>();
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsActive { get; set; } = true;
    public string? ImageUrl { get; set; }
}

