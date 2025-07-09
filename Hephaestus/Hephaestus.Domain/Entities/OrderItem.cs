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
    public List<Customization> Customizations { get; set; } = new List<Customization>();
    public List<string> AdditionalIds { get; set; } = new List<string>();
    public List<string> Tags { get; set; } = new List<string>();
}