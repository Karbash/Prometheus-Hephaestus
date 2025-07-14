namespace Hephaestus.Application.DTOs.Response;

public class SalesByTenantResponse
{
    public string TenantId { get; set; } = string.Empty;
    public double TotalAmount { get; set; }
    public double PlatformFee { get; set; }
}