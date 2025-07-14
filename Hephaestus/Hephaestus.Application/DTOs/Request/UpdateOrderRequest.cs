using Hephaestus.Domain.Enum;
using System.Collections.Generic;

namespace Hephaestus.Application.DTOs.Request;

public class UpdateOrderRequest
{
    public string CustomerPhoneNumber { get; set; } = string.Empty;
    public List<OrderItemRequest> Items { get; set; } = new List<OrderItemRequest>();
    public string? PromotionId { get; set; }
    public string? CouponId { get; set; }
    public OrderStatus Status { get; set; }
    public PaymentStatus PaymentStatus { get; set; }
}