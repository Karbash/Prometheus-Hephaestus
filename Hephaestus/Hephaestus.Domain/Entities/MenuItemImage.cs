namespace Hephaestus.Domain.Entities;

public class MenuItemImage
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string MenuItemId { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
