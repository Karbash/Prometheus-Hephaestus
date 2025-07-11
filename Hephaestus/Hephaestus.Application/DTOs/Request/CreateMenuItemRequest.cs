using System.Collections.Generic;

namespace Hephaestus.Application.DTOs.Request;

public class CreateMenuItemRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string CategoryId { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public bool IsAvailable { get; set; } = true;
    public List<string> TagIds { get; set; } = new List<string>(); // Alterado de Tags para TagIds
    public List<string> AvailableAdditionalIds { get; set; } = new List<string>();
    public string? ImageUrl { get; set; }
}