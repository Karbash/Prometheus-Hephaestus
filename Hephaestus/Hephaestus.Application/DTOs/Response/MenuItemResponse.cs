namespace Hephaestus.Application.DTOs.Response;

public class MenuItemResponse
{
    public string Id { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string CategoryId { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public bool IsAvailable { get; set; }
    public List<string> Tags { get; set; } = new List<string>(); // Nomes das tags
    public List<string> AvailableAdditionalIds { get; set; } = new List<string>();
    public string? ImageUrl { get; set; }
}