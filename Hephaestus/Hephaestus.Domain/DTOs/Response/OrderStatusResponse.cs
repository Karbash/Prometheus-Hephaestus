using Hephaestus.Domain.Enum;

namespace Hephaestus.Domain.DTOs.Response
{
    public class OrderStatusResponse
    {
        public string OrderId { get; set; } = string.Empty;
        public OrderStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
