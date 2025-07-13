namespace Hephaestus.Application.DTOs.Request;

public class CreateAdditionalRequest
{
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
}