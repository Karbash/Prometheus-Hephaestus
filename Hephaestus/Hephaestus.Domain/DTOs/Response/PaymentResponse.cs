using Hephaestus.Domain.Enum;

namespace Hephaestus.Domain.DTOs.Response;

public class PaymentResponse
{
    public string OrderId { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public decimal PlatformFee { get; set; }
    public PaymentStatus PaymentStatus { get; set; }
}