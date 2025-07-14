namespace Hephaestus.Application.DTOs.Request
{
    public class CreateOrderRequest
    {
        public string CustomerPhoneNumber { get; set; } = string.Empty;
        public List<OrderItemRequest> Items { get; set; } = new List<OrderItemRequest>();
        public string? PromotionId { get; set; }
        public string? CouponId { get; set; }
    }
}
