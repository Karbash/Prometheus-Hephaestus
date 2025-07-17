namespace Hephaestus.Domain.Entities;

public class OrderItemAdditional
{
    public string OrderItemId { get; set; } = string.Empty;
    public string AdditionalId { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public OrderItem OrderItem { get; set; } = null!;
    public Additional Additional { get; set; } = null!;
} 