namespace Hephaestus.Application.DTOs.Response;

public class CompanyResponse
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public bool IsEnabled { get; set; }
    public string FeeType { get; set; } = "Percentage";
    public double FeeValue { get; set; }
}