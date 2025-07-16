namespace Hephaestus.Domain.DTOs.Request;

public class CustomerRequest
{
    public string? Id { get; set; }
    public string PhoneNumber { get; set; } = string.Empty;
    public string? Name { get; set; }
    public string State { get; set; } = string.Empty; // Novo campo obrigat�rio
    public string? City { get; set; }
    public string? Street { get; set; }
    public string? Number { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
}
