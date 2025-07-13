namespace Hephaestus.Application.DTOs.Request;

public class UpdateAdditionalRequest
{
    public string Id { get; set; } = string.Empty;
    public string? Name { get; set; }
    public decimal? Price { get; set; }
}