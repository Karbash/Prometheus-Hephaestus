namespace Hephaestus.Domain.Entities;

public class MenuItemAdditional
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string CompanyId { get; set; } = string.Empty;
    public string MenuItemId { get; set; } = string.Empty;
    public string AdditionalId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public MenuItem MenuItem { get; set; } = null!;
    public Additional Additional { get; set; } = null!;
} 