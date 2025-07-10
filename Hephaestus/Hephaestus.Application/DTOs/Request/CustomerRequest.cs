namespace Hephaestus.Application.DTOs.Request;
public class CustomerRequest
{
    public string PhoneNumber { get; set; } = string.Empty;
    public string? Name { get; set; } = string.Empty;
    public string? Address { get; set; } = string.Empty;
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
}