namespace Hephaestus.Domain.Entities;

public class Category
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string TenantId { get; set; } = string.Empty; // Vazio para categorias globais
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ParentCategoryId { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsGlobal { get; set; } = false; // Indica se é uma categoria global
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }
} 