namespace Hephaestus.Domain.Entities;

public class CompanyImage
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string CompanyId { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public string ImageType { get; set; } = string.Empty; // Ex.: "Logo", "Banner", "Gallery"
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}