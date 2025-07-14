namespace Hephaestus.Application.DTOs.Request
{
    public class OrderItemRequest
    {
        public string MenuItemId { get; set; } = string.Empty;
        public List<string>? Customizations { get; set; }
        public List<string>? AdditionalIds { get; set; }
    }
}
