namespace Hephaestus.Domain.Entities;

/// <summary>
/// Contexto de conversa para manter histórico e estado
/// </summary>
public class ConversationContext
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string PhoneNumber { get; set; } = string.Empty;
    public string? CurrentIntent { get; set; }
    public string? LastMessage { get; set; }
    public Dictionary<string, object>? ContextData { get; set; }
    public DateTime LastInteraction { get; set; } = DateTime.UtcNow;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;
} 