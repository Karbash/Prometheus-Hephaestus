using Hephaestus.Domain.Entities;
using Hephaestus.Domain.Repositories;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;

namespace Hephaestus.Application.Services;

/// <summary>
/// Implementação do serviço de contexto de conversa
/// </summary>
public class ConversationContextService : IConversationContextService
{
    private readonly ILogger<ConversationContextService> _logger;
    private readonly IConversationSessionRepository _sessionRepository;

    public ConversationContextService(
        IConversationSessionRepository sessionRepository,
        ILogger<ConversationContextService> logger)
    {
        _sessionRepository = sessionRepository;
        _logger = logger;
    }

    public async Task<ConversationSession> GetOrCreateSessionAsync(string sessionId, string phoneNumber)
    {
        var existingSession = await _sessionRepository.GetByIdAsync(sessionId);
        
        if (existingSession != null)
        {
            existingSession.LastActivity = DateTime.UtcNow;
            await _sessionRepository.SaveAsync(existingSession);
            return existingSession;
        }

        var newSession = new ConversationSession
        {
            SessionId = sessionId,
            PhoneNumber = phoneNumber,
            CreatedAt = DateTime.UtcNow,
            LastActivity = DateTime.UtcNow
        };

        await _sessionRepository.SaveAsync(newSession);
        _logger.LogInformation("Nova sessão criada: {SessionId} para {PhoneNumber}", sessionId, phoneNumber);
        return newSession;
    }

    public async Task<(bool canSkipOpenAI, string? intent, string? response)> CanSkipOpenAIAsync(string message, ConversationSession session)
    {
        var normalizedMessage = message.ToLower().Trim();
        
        // 1. Confirmações simples
        if (IsConfirmation(normalizedMessage))
        {
            return (true, "confirmation", "Entendi! Posso ajudar com mais alguma coisa?");
        }

        // 2. Continuações de conversa
        if (IsContinuation(normalizedMessage, session))
        {
            return HandleContinuation(normalizedMessage, session);
        }

        // 3. Referências a dados já fornecidos
        if (IsReference(normalizedMessage, session))
        {
            return HandleReference(normalizedMessage, session);
        }

        // 4. Perguntas simples sobre dados já carregados
        if (IsSimpleQuestion(normalizedMessage, session))
        {
            return HandleSimpleQuestion(normalizedMessage, session);
        }

        // 5. Navegação em listas
        if (IsNavigation(normalizedMessage, session))
        {
            return HandleNavigation(normalizedMessage, session);
        }

        return (false, null, null);
    }

    public async Task UpdateSessionContextAsync(string sessionId, string intent, Dictionary<string, object>? contextData = null)
    {
        var session = await _sessionRepository.GetByIdAsync(sessionId);
        
        if (session != null)
        {
            session.LastIntent = intent;
            session.LastActivity = DateTime.UtcNow;
            
            if (contextData != null)
            {
                foreach (var kvp in contextData)
                {
                    session.ContextData[kvp.Key] = kvp.Value;
                }
            }
            
            await _sessionRepository.SaveAsync(session);
            _logger.LogInformation("Contexto atualizado para sessão {SessionId}: {Intent}", sessionId, intent);
        }
    }

    public async Task AddMessageAsync(string sessionId, string message, string? intent = null, string? response = null, bool usedOpenAI = false)
    {
        var conversationMessage = new ConversationMessage
        {
            SessionId = sessionId,
            Message = message,
            Intent = intent,
            Response = response,
            UsedOpenAI = usedOpenAI,
            Timestamp = DateTime.UtcNow
        };

        await _sessionRepository.AddMessageAsync(sessionId, conversationMessage);
    }

    public async Task CleanupOldSessionsAsync(TimeSpan maxAge)
    {
        var cutoffTime = DateTime.UtcNow.Subtract(maxAge);
        var oldSessions = await _sessionRepository.GetOldSessionsAsync(cutoffTime);

        foreach (var session in oldSessions)
        {
            await _sessionRepository.DeleteAsync(session.SessionId);
            _logger.LogInformation("Sessão removida por inatividade: {SessionId}", session.SessionId);
        }
    }

    #region Private Methods

    private bool IsConfirmation(string message)
    {
        var confirmations = new[] { "sim", "ok", "certo", "beleza", "tá", "blz", "👍", "✅" };
        return confirmations.Any(c => message.Contains(c));
    }

    private bool IsContinuation(string message, ConversationSession session)
    {
        var continuations = new[] { "mais", "outros", "próximo", "seguinte", "continua", "mais opções" };
        return continuations.Any(c => message.Contains(c)) && !string.IsNullOrEmpty(session.LastIntent);
    }

    private bool IsReference(string message, ConversationSession session)
    {
        var references = new[] { "primeiro", "segundo", "terceiro", "1", "2", "3", "esse", "aquele" };
        return references.Any(r => message.Contains(r)) && session.ContextData.ContainsKey("found_companies");
    }

    private bool IsSimpleQuestion(string message, ConversationSession session)
    {
        var questions = new[] { "telefone", "fone", "tel", "endereço", "onde", "horário", "preço", "valor" };
        return questions.Any(q => message.Contains(q)) && session.ContextData.ContainsKey("found_companies");
    }

    private bool IsNavigation(string message, ConversationSession session)
    {
        var navigation = new[] { "anterior", "próximo", "voltar", "avançar", "página", "página seguinte" };
        return navigation.Any(n => message.Contains(n));
    }

    private (bool canSkipOpenAI, string? intent, string? response) HandleContinuation(string message, ConversationSession session)
    {
        switch (session.LastIntent)
        {
            case "buscar_restaurantes":
                return (true, "buscar_mais_restaurantes", "Vou buscar mais restaurantes próximos...");
            case "buscar_categorias":
                return (true, "buscar_mais_categorias", "Aqui estão mais categorias disponíveis...");
            case "buscar_promocoes":
                return (true, "buscar_mais_promocoes", "Vou mostrar mais promoções ativas...");
            default:
                return (true, "continuar_conversa", "Posso ajudar com mais alguma coisa?");
        }
    }

    private (bool canSkipOpenAI, string? intent, string? response) HandleReference(string message, ConversationSession session)
    {
        if (session.ContextData.TryGetValue("found_companies", out var companies) && companies is List<string> companyList)
        {
            if (message.Contains("primeiro") || message.Contains("1"))
            {
                return (true, "detalhes_restaurante", $"Detalhes do primeiro restaurante: {companyList.FirstOrDefault()}");
            }
            if (message.Contains("segundo") || message.Contains("2"))
            {
                return (true, "detalhes_restaurante", $"Detalhes do segundo restaurante: {companyList.Skip(1).FirstOrDefault()}");
            }
        }
        
        return (true, "referencia_nao_encontrada", "Desculpe, não encontrei essa referência. Pode ser mais específico?");
    }

    private (bool canSkipOpenAI, string? intent, string? response) HandleSimpleQuestion(string message, ConversationSession session)
    {
        if (message.Contains("telefone") || message.Contains("fone") || message.Contains("tel"))
        {
            return (true, "mostrar_telefone", "O telefone do restaurante é: (11) 9999-9999");
        }
        
        if (message.Contains("endereço") || message.Contains("onde"))
        {
            return (true, "mostrar_endereco", "O endereço é: Rua das Flores, 123 - Centro");
        }
        
        if (message.Contains("horário"))
        {
            return (true, "mostrar_horario", "O horário de funcionamento é: 11h às 22h");
        }
        
        return (true, "pergunta_simples", "Posso ajudar com mais alguma informação?");
    }

    private (bool canSkipOpenAI, string? intent, string? response) HandleNavigation(string message, ConversationSession session)
    {
        if (message.Contains("próximo") || message.Contains("avançar"))
        {
            return (true, "proxima_pagina", "Aqui está a próxima página de resultados...");
        }
        
        if (message.Contains("anterior") || message.Contains("voltar"))
        {
            return (true, "pagina_anterior", "Aqui está a página anterior...");
        }
        
        return (true, "navegacao", "Navegando pelos resultados...");
    }

    #endregion
} 