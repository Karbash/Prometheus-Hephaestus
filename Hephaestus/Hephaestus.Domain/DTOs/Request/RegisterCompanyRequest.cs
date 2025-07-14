using Hephaestus.Domain.Enum;

namespace Hephaestus.Application.DTOs.Request;

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
    public string State { get; set; } = string.Empty;
    public string? City { get; set; }
    public string? Neighborhood { get; set; } // Novo campo
    public string? Street { get; set; }
    public string? Number { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? Slogan { get; set; }
    public string? Description { get; set; }
}