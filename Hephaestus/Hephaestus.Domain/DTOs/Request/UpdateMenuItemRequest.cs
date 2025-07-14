namespace Hephaestus.Application.DTOs.Request;

public class UpdateMenuItemRequest
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? CategoryId { get; set; }
    public decimal? Price { get; set; }
    public bool? IsAvailable { get; set; }
    public List<string>? TagIds { get; set; } // Alterado de Tags para TagIds
    public List<string>? AvailableAdditionalIds { get; set; }
    public string? ImageUrl { get; set; }
}