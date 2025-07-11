using Hephaestus.Domain.Enum;

namespace Hephaestus.Application.DTOs.Request;

public class UpdateCompanyRequest
{
    public string Id { get; set; } = string.Empty;
    public string? Name { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string? ApiKey { get; set; }
    public FeeType? FeeType { get; set; }
    public decimal? FeeValue { get; set; }
    public bool? IsEnabled { get; set; }
    public string? State { get; set; } 
    public string? City { get; set; }
    public string? Street { get; set; }
    public string? Number { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? Slogan { get; set; }
    public string? Description { get; set; }
}