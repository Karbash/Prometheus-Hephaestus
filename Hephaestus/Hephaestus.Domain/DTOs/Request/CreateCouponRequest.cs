namespace Hephaestus.Domain.DTOs.Request;

public class CreateCouponRequest
{
    public string Code { get; set; } = string.Empty;
    public string? CustomerPhoneNumber { get; set; }
    public string DiscountType { get; set; } = string.Empty;
    public decimal DiscountValue { get; set; }
    public string? MenuItemId { get; set; }
    public decimal MinOrderValue { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}