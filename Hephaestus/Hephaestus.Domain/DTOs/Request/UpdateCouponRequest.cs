using Hephaestus.Domain.Enum;

namespace Hephaestus.Domain.DTOs.Request;

public class UpdateCouponRequest
{
    public string Code { get; set; } = string.Empty;
    public string? CustomerPhoneNumber { get; set; }
    public DiscountType DiscountType { get; set; }
    public decimal DiscountValue { get; set; }
    public string? MenuItemId { get; set; }
    public decimal MinOrderValue { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsActive { get; set; }
}
