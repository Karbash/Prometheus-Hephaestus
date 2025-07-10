namespace Hephaestus.Application.DTOs.Request;

public class CompanyRequest
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public string FeeType { get; set; } = "Percentage";
    public double FeeValue { get; set; }
    public bool IsEnabled { get; set; }
}