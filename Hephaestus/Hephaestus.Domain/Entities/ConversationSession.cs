using System.Text.Json;

namespace Hephaestus.Domain.Entities;

/// <summary>
/// Entidade para gerenciar sessões de conversa e otimizar uso da OpenAI
/// </summary>
public class ConversationSession
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string SessionId { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string? LastIntent { get; set; }
    public string? ConversationStep { get; set; }
    public DateTime LastActivity { get; set; } = DateTime.UtcNow;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;
    
    /// <summary>
    /// Histórico de mensagens para contexto
    /// </summary>
    public List<ConversationMessage> Messages { get; set; } = new();
    
    /// <summary>
    /// Dados específicos da sessão (restaurantes encontrados, pedidos, etc.)
    /// </summary>
    public string? SessionDataJson { get; set; }
    
    /// <summary>
    /// Dados específicos da sessão como Dictionary (não mapeado no EF)
    /// </summary>
    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public Dictionary<string, object> ContextData 
    { 
        get => !string.IsNullOrEmpty(SessionDataJson) 
            ? JsonSerializer.Deserialize<Dictionary<string, object>>(SessionDataJson) ?? new()
            : new();
        set => SessionDataJson = JsonSerializer.Serialize(value);
    }
}

/// <summary>
/// Mensagem individual da conversa
/// </summary>
public class ConversationMessage
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string SessionId { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? Intent { get; set; }
    public string? Response { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public bool UsedOpenAI { get; set; } = false;
} 