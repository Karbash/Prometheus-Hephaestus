using Hephaestus.Domain.Entities;

namespace Hephaestus.Application.Services;

/// <summary>
/// Serviço para gerenciar contexto de conversas e otimizar uso da OpenAI
/// </summary>
public interface IConversationContextService
{
    /// <summary>
    /// Obtém ou cria uma sessão de conversa
    /// </summary>
    Task<ConversationSession> GetOrCreateSessionAsync(string sessionId, string phoneNumber);
    
    /// <summary>
    /// Verifica se a mensagem pode ser respondida sem OpenAI
    /// </summary>
    Task<(bool canSkipOpenAI, string? intent, string? response)> CanSkipOpenAIAsync(string message, ConversationSession session);
    
    /// <summary>
    /// Atualiza o contexto da sessão
    /// </summary>
    Task UpdateSessionContextAsync(string sessionId, string intent, Dictionary<string, object>? contextData = null);
    
    /// <summary>
    /// Adiciona uma mensagem ao histórico
    /// </summary>
    Task AddMessageAsync(string sessionId, string message, string? intent = null, string? response = null, bool usedOpenAI = false);
    
    /// <summary>
    /// Limpa sessões antigas
    /// </summary>
    Task CleanupOldSessionsAsync(TimeSpan maxAge);
} 