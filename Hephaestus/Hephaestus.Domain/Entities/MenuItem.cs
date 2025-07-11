namespace Hephaestus.Domain.Entities;

public class MenuItem
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string TenantId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string CategoryId { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public bool IsAvailable { get; set; }
    public List<string> AvailableAdditionalIds { get; set; } = new List<string>();
    public string? ImageUrl { get; set; }
    public List<MenuItemTag> MenuItemTags { get; set; } = new List<MenuItemTag>();
}