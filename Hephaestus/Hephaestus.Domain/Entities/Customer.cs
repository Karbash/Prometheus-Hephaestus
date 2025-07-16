namespace Hephaestus.Domain.Entities;

public class Customer
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string TenantId { get; set; } = string.Empty;
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
    public string AddressId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }
}