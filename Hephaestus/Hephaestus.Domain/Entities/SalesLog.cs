using Hephaestus.Domain.Enum;

namespace Hephaestus.Domain.Entities;

public class SalesLog
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string TenantId { get; set; } = string.Empty;
    public string CustomerId { get; set; } = string.Empty;
    public string CompanyId { get; set; } = string.Empty;
    public string OrderId { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public decimal PlatformFee { get; set; }
    public string? PromotionId { get; set; }
    public string? CouponId { get; set; }
    public PaymentStatus PaymentStatus { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}