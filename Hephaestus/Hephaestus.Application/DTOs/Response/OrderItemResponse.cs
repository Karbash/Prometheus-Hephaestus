namespace Hephaestus.Application.DTOs.Response
{
    public class OrderItemResponse
    {
        public string MenuItemId { get; set; } = string.Empty;
        public List<string>? Customizations { get; set; }
        public List<string>? AdditionalIds { get; set; }
    }
}
