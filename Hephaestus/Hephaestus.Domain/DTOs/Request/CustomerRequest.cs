namespace Hephaestus.Domain.DTOs.Request;

public class CustomerRequest
{
    public string? Id { get; set; }
    public string PhoneNumber { get; set; } = string.Empty;
    public string? Name { get; set; }
    public string Email { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public bool EmailVerified { get; set; } = false;
    public bool PhoneVerified { get; set; } = false;
    public bool MfaEnabled { get; set; } = false;
    public string? PreferredPaymentMethod { get; set; }
    public string? DietaryPreferences { get; set; }
    public string? Allergies { get; set; }
    public DateTime? BirthDate { get; set; }
    public string? Gender { get; set; }
    public string LanguagePreference { get; set; } = "pt-BR";
    public string TimeZone { get; set; } = "America/Sao_Paulo";
    public string NotificationPreferences { get; set; } = "email,sms";
    public string State { get; set; } = string.Empty; // Campo obrigatório para endereço
    public string? City { get; set; }
    public string? Street { get; set; }
    public string? Number { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
}
