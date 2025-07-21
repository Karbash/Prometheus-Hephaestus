namespace Hephaestus.Domain.DTOs.Response;

/// <summary>
/// Resposta para mensagens do WhatsApp Business API
/// </summary>
public class WhatsAppResponse
{
    /// <summary>
    /// Mensagem a ser enviada ao usuário
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Códigos de ações a serem executadas (pipeline)
    /// </summary>
    public List<int> Codes { get; set; } = new List<int>();

    /// <summary>
    /// Dados adicionais para as ações
    /// </summary>
    public Dictionary<string, object>? Data { get; set; }

    /// <summary>
    /// Indica se deve aguardar resposta do usuário
    /// </summary>
    public bool WaitForResponse { get; set; } = false;

    /// <summary>
    /// Contexto da conversa para próxima interação
    /// </summary>
    public string? ConversationContext { get; set; }
} 