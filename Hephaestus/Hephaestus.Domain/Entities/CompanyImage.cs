namespace Hephaestus.Domain.Entities;

public class CompanyImage
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string CompanyId { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public string ImageType { get; set; } = string.Empty; // Ex.: "Logo", "Banner", "Gallery"
    public bool IsMain { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}