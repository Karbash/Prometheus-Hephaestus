using Hephaestus.Domain.Entities;

namespace Hephaestus.Domain.DTOs.Request
{
    public class OrderItemRequest
    {
        public string MenuItemId { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public string? Notes { get; set; }
        public List<string>? AdditionalIds { get; set; }
        public List<string>? Tags { get; set; }
        public List<Customization>? Customizations { get; set; }
    }
}
