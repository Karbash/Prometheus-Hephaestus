namespace Hephaestus.Domain.DTOs.Request;

public class CreateMenuItemRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string CategoryId { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public bool IsAvailable { get; set; } = true;
    public List<string> TagIds { get; set; } = new List<string>(); // Alterado de Tags para TagIds
    public string? ImageUrl { get; set; }
}
