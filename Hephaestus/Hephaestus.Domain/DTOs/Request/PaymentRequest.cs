namespace Hephaestus.Domain.DTOs.Request;

public class PaymentRequest
{
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string PaymentMethod { get; set; } = string.Empty;
    public string CustomerId { get; set; } = string.Empty;
    public string OrderId { get; set; } = string.Empty;
}