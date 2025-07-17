namespace Hephaestus.Domain.Entities;

public class OrderItem
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string TenantId { get; set; } = string.Empty;
    public string OrderId { get; set; } = string.Empty;
    public string MenuItemId { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public string Notes { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public List<Customization> Customizations { get; set; } = new List<Customization>();
    public string AdditionalIds { get; set; } = string.Empty;
    public string Tags { get; set; } = string.Empty;
    public Order? Order { get; set; } // Propriedade de navegação para Order (agora anulável)
}