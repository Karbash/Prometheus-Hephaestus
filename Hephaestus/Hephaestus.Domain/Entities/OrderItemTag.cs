namespace Hephaestus.Domain.Entities;

public class OrderItemTag
{
    public string OrderItemId { get; set; } = string.Empty;
    public string TagId { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public OrderItem OrderItem { get; set; } = null!;
    public Tag Tag { get; set; } = null!;
} 