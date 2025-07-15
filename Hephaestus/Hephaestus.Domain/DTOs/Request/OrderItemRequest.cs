using Hephaestus.Domain.Entities;
using Hephaestus.Domain.Enum;

namespace Hephaestus.Domain.DTOs.Request
{
    public class OrderItemRequest
    {
        public string MenuItemId { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public string? Notes { get; set; }
        public string? Id { get; set; }
        public List<string>? AdditionalIds { get; set; }
        public List<string>? Tags { get; set; }
        public List<CustomizationRequest>? Customizations { get; set; }
    }

    public class CustomizationRequest
    {
        public CustomizationType Type { get; set; }
        public string Value { get; set; } = string.Empty;
    }
}
