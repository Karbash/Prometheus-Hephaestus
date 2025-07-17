using Hephaestus.Domain.Enum;

namespace Hephaestus.Domain.DTOs.Request;

public class UpdateCompanyRequest
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string ApiKey { get; set; } = string.Empty;
    public FeeType FeeType { get; set; } = FeeType.Percentage;
    public double FeeValue { get; set; }
    public bool IsEnabled { get; set; }
    public AddressRequest Address { get; set; } = new AddressRequest();
    public string? Slogan { get; set; }
    public string? Description { get; set; }
}
