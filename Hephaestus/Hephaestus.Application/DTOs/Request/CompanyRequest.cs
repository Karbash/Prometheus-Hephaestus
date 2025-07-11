using Hephaestus.Domain.Enum;

namespace Hephaestus.Application.DTOs.Request;

public class CompanyRequest
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string ApiKey { get; set; } = string.Empty;
    public string FeeType { get; set; } = "Percentage";
    public double FeeValue { get; set; }
    public bool IsEnabled { get; set; }
    public string State { get; set; } = string.Empty; // Novo campo obrigatório
    public string? City { get; set; }
    public string? Street { get; set; }
    public string? Number { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? Slogan { get; set; }
    public string? Description { get; set; }
}