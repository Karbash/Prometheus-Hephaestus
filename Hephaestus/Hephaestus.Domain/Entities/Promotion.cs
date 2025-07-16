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
    public int? MaxTotalUses { get; set; }
    public int? MaxUsesPerCustomer { get; set; }
    public string DaysOfWeek { get; set; } = string.Empty; // Ex: "Mon,Tue,Wed"
    public string Hours { get; set; } = string.Empty; // Ex: "11:00-15:00"
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsActive { get; set; } = true;
    public string? ImageUrl { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }
}

