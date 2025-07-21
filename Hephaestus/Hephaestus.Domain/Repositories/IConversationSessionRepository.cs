using Hephaestus.Domain.Entities;

namespace Hephaestus.Domain.Repositories;

/// <summary>
/// Repositório para gerenciar sessões de conversa persistentes
/// </summary>
public interface IConversationSessionRepository
{
    /// <summary>
    /// Busca uma sessão por ID
    /// </summary>
    Task<ConversationSession?> GetByIdAsync(string sessionId);
    
    /// <summary>
    /// Busca uma sessão por número de telefone
    /// </summary>
    Task<ConversationSession?> GetByPhoneNumberAsync(string phoneNumber);
    
    /// <summary>
    /// Salva ou atualiza uma sessão
    /// </summary>
    Task SaveAsync(ConversationSession session);
    
    /// <summary>
    /// Remove uma sessão
    /// </summary>
    Task DeleteAsync(string sessionId);
    
    /// <summary>
    /// Busca sessões antigas para limpeza
    /// </summary>
    Task<IEnumerable<ConversationSession>> GetOldSessionsAsync(DateTime cutoffTime);
    
    /// <summary>
    /// Adiciona uma mensagem à sessão
    /// </summary>
    Task AddMessageAsync(string sessionId, ConversationMessage message);
    
    /// <summary>
    /// Busca mensagens de uma sessão
    /// </summary>
    Task<IEnumerable<ConversationMessage>> GetMessagesAsync(string sessionId, int limit = 10);
} 