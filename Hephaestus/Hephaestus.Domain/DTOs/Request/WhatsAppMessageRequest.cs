namespace Hephaestus.Domain.DTOs.Request;

/// <summary>
/// Requisição para processamento de mensagens do WhatsApp Business API
/// </summary>
public class WhatsAppMessageRequest
{
    /// <summary>
    /// ID da mensagem do WhatsApp
    /// </summary>
    public string MessageId { get; set; } = string.Empty;

    /// <summary>
    /// Número do telefone do usuário
    /// </summary>
    public string From { get; set; } = string.Empty;

    /// <summary>
    /// Número do telefone do WhatsApp Business
    /// </summary>
    public string To { get; set; } = string.Empty;

    /// <summary>
    /// Tipo da mensagem (text, location, image, etc.)
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Conteúdo da mensagem de texto
    /// </summary>
    public string? Text { get; set; }

    /// <summary>
    /// Localização (latitude)
    /// </summary>
    public double? Latitude { get; set; }

    /// <summary>
    /// Localização (longitude)
    /// </summary>
    public double? Longitude { get; set; }

    /// <summary>
    /// Timestamp da mensagem
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Contexto da conversa (para manter histórico)
    /// </summary>
    public string? ConversationId { get; set; }

    /// <summary>
    /// Dados adicionais do contexto (localização, etc.)
    /// </summary>
    public Dictionary<string, object>? ContextData { get; set; }
} 