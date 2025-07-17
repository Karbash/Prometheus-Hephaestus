namespace Hephaestus.Domain.DTOs.Request
{
    public class CreateOrderRequest
    {
        public string CustomerPhoneNumber { get; set; } = string.Empty;
        public string? CouponId { get; set; }
        public string? PromotionId { get; set; }
        public List<OrderItemRequest> Items { get; set; } = new List<OrderItemRequest>();
        public AddressRequest Address { get; set; } = new AddressRequest();
    }
}
