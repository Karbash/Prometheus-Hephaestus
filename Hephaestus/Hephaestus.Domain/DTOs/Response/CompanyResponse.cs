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