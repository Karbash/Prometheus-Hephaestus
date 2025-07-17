namespace Hephaestus.Domain.DTOs.Response;

public class MenuItemResponse
{
    public string Id { get; set; } = string.Empty;
    public string CompanyId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string CategoryId { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public bool IsAvailable { get; set; }
    public List<string> TagIds { get; set; } = new List<string>(); // Alterado de Tags para TagIds
    public string? ImageUrl { get; set; }
    public List<TagResponse> Tags { get; set; } = new List<TagResponse>();
    public List<AdditionalResponse> Additionals { get; set; } = new List<AdditionalResponse>();
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
} 
