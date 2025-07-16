using Hephaestus.Domain.Enum;

namespace Hephaestus.Domain.Entities;

public class Coupon
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string TenantId { get; set; } = string.Empty;
    public string? CustomerPhoneNumber { get; set; }
    public string Code { get; set; } = string.Empty;
    public DiscountType DiscountType { get; set; }
    public decimal DiscountValue { get; set; }
    public string? MenuItemId { get; set; }
    public decimal? MinOrderValue { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public int? MaxTotalUses { get; set; }
    public int? MaxUsesPerCustomer { get; set; }

}
