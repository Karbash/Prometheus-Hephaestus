using Hephaestus.Domain.Enum;
using System.Collections.Generic;

namespace Hephaestus.Domain.DTOs.Request;

public class UpdateOrderRequest
{
    public string OrderId { get; set; } = string.Empty;
    public string? CustomerPhoneNumber { get; set; } // Opcional
    public string? CouponId { get; set; } // Opcional
    public string? PromotionId { get; set; } // Opcional
    public OrderStatus? Status { get; set; } // Opcional
    public PaymentStatus? PaymentStatus { get; set; } // Opcional
    public List<OrderItemRequest>? Items { get; set; } // Opcional
}
