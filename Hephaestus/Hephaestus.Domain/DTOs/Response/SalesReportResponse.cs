namespace Hephaestus.Application.DTOs.Response;

public class SalesReportResponse
{
    public double TotalSales { get; set; }
    public double TotalPlatformFees { get; set; }
    public List<SalesByTenantResponse> SalesByTenant { get; set; } = [];
    public int TotalPromotionsApplied { get; set; }
}