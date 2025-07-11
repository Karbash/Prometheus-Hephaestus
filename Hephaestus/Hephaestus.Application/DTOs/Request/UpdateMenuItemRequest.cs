namespace Hephaestus.Application.DTOs.Request;

/// <summary>
/// DTO para atualização de um item do cardápio.
/// </summary>
public class UpdateMenuItemRequest
{
    public string Id { get; set; } = string.Empty;
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? CategoryId { get; set; }
    public decimal? Price { get; set; }
    public bool? IsAvailable { get; set; }
    public List<string>? Tags { get; set; }
    public List<string>? AvailableAdditionalIds { get; set; }
    public string? ImageUrl { get; set; }
}