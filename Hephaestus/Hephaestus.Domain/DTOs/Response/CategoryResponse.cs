namespace Hephaestus.Domain.DTOs.Response;

public class CategoryResponse
{
    public string Id { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public bool IsGlobal { get; set; } = false; // Indica se é uma categoria global
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string? CompanyId { get; set; }
} 
