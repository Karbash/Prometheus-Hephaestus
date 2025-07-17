using Hephaestus.Domain.Enum;

namespace Hephaestus.Domain.DTOs.Request;

public class RegisterCompanyRequest
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string Password { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public FeeType FeeType { get; set; } = FeeType.Fixed;
    public decimal FeeValue { get; set; } = 0;
    public bool IsEnabled { get; set; } = false;
    public AddressRequest Address { get; set; } = new AddressRequest();
    public string? Slogan { get; set; }
    public string? Description { get; set; }
}
