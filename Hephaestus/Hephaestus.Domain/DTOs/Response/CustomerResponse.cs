namespace Hephaestus.Domain.DTOs.Response;

public class CustomerResponse
{
    public string Id { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string? Name { get; set; }
    public string? Email { get; set; } // Adicionando campo Email
    public string State { get; set; } = string.Empty; // Novo campo obrigatório
    public string? City { get; set; }
    public string? Street { get; set; }
    public string? Number { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public DateTime CreatedAt { get; set; }
} 
