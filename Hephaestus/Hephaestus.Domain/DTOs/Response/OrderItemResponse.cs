namespace Hephaestus.Domain.DTOs.Response
{
    public class OrderItemResponse
    {
        public string Id { get; set; } = string.Empty;
        public string MenuItemId { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public string? Notes { get; set; }
        public List<string>? Tags { get; set; }
        public List<string>? AdditionalIds { get; set; }
        public List<CustomizationResponse>? Customizations { get; set; }
    }

    public class CustomizationResponse
    {
        public Hephaestus.Domain.Enum.CustomizationType Type { get; set; }
        public string Value { get; set; } = string.Empty;
    }
}
