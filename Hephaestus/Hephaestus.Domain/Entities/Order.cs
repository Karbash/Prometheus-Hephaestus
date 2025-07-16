using Hephaestus.Domain.Enum;

namespace Hephaestus.Domain.Entities;

public class Order
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string TenantId { get; set; } = string.Empty;
    public string CustomerPhoneNumber { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public decimal PlatformFee { get; set; }
    public string? PromotionId { get; set; }
    public string? CouponId { get; set; }
    public OrderStatus Status { get; set; }
    public PaymentStatus PaymentStatus { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}
