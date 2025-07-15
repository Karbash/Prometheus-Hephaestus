using Hephaestus.Domain.Enum;

namespace Hephaestus.Domain.DTOs.Response;

public class PaymentResponse
{
    public string PaymentId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string PaymentMethod { get; set; } = string.Empty;
    public string CustomerId { get; set; } = string.Empty;
    public string OrderId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}