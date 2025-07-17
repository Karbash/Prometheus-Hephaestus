namespace Hephaestus.Domain.DTOs.Request;

public class CreateReviewRequest
{
    public string TenantId { get; set; } = string.Empty;
    public string OrderId { get; set; } = string.Empty;
    public string CustomerId { get; set; } = string.Empty;
    public int Rating { get; set; }
    public string? Comment { get; set; }
} 