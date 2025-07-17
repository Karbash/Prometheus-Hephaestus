using Hephaestus.Domain.Enum;

namespace Hephaestus.Domain.DTOs.Response;

public class CompanyResponse
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public bool IsEnabled { get; set; }
    public FeeType FeeType { get; set; }
    public double FeeValue { get; set; }
    public AddressResponse? Address { get; set; }
    public string? Slogan { get; set; }
    public string? Description { get; set; }
}
