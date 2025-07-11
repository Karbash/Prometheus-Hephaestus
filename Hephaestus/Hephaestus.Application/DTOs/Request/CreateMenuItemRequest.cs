namespace Hephaestus.Application.DTOs.Request;

/// <summary>
/// DTO para criação de um item do cardápio.
/// </summary>
public class CreateMenuItemRequest
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string CategoryId { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public bool IsAvailable { get; set; }
    public List<string> Tags { get; set; } = new List<string>();
    public List<string> AvailableAdditionalIds { get; set; } = new List<string>();
    public string? ImageUrl { get; set; }
}