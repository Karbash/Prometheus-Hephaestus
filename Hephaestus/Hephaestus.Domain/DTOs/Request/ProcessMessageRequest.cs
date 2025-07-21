namespace Hephaestus.Domain.DTOs.Request;

/// <summary>
/// DTO para processamento de mensagens de chat
/// </summary>
public class ProcessMessageRequest
{
    /// <summary>
    /// Número de telefone do usuário
    /// </summary>
    public string PhoneNumber { get; set; } = string.Empty;
    
    /// <summary>
    /// Mensagem do usuário
    /// </summary>
    public string Message { get; set; } = string.Empty;
    
    /// <summary>
    /// Dados adicionais para contexto (opcional)
    /// </summary>
    public Dictionary<string, object>? ContextData { get; set; }
    
    /// <summary>
    /// ID da sessão/conversa (opcional)
    /// </summary>
    public string? SessionId { get; set; }
} 