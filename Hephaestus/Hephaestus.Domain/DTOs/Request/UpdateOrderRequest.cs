using Hephaestus.Domain.Enum;
using System.Collections.Generic;

namespace Hephaestus.Domain.DTOs.Request;

public class UpdateOrderRequest
{
    public string CustomerPhoneNumber { get; set; } = string.Empty;
    public string? CouponId { get; set; }
    public string? PromotionId { get; set; }
    public OrderStatus Status { get; set; }
    public PaymentStatus PaymentStatus { get; set; }
    public List<OrderItemRequest> Items { get; set; } = new List<OrderItemRequest>();
}