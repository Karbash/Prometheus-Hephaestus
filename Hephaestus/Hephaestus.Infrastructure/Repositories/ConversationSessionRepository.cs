using Hephaestus.Domain.Entities;
using Hephaestus.Domain.Repositories;
using Hephaestus.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Hephaestus.Infrastructure.Repositories;

/// <summary>
/// Implementação do repositório de sessões de conversa
/// </summary>
public class ConversationSessionRepository : IConversationSessionRepository
{
    private readonly HephaestusDbContext _context;
    private readonly ILogger<ConversationSessionRepository> _logger;

    public ConversationSessionRepository(HephaestusDbContext context, ILogger<ConversationSessionRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<ConversationSession?> GetByIdAsync(string sessionId)
    {
        return await _context.ConversationSessions
            .Include(s => s.Messages.OrderByDescending(m => m.Timestamp).Take(10))
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.SessionId == sessionId);
    }

    public async Task<ConversationSession?> GetByPhoneNumberAsync(string phoneNumber)
    {
        return await _context.ConversationSessions
            .Include(s => s.Messages.OrderByDescending(m => m.Timestamp).Take(10))
            .AsNoTracking()
            .Where(s => s.PhoneNumber == phoneNumber && s.IsActive)
            .OrderByDescending(s => s.LastActivity)
            .FirstOrDefaultAsync();
    }

    public async Task SaveAsync(ConversationSession session)
    {
        var existingSession = await _context.ConversationSessions
            .FirstOrDefaultAsync(s => s.SessionId == session.SessionId);

        if (existingSession != null)
        {
            // Atualiza sessão existente
            existingSession.LastIntent = session.LastIntent;
            existingSession.ConversationStep = session.ConversationStep;
            existingSession.SessionDataJson = session.SessionDataJson;
            existingSession.LastActivity = session.LastActivity;
            existingSession.IsActive = session.IsActive;

            _context.ConversationSessions.Update(existingSession);
        }
        else
        {
            // Cria nova sessão
            await _context.ConversationSessions.AddAsync(session);
        }

        await _context.SaveChangesAsync();
        _logger.LogInformation("Sessão salva: {SessionId}", session.SessionId);
    }

    public async Task DeleteAsync(string sessionId)
    {
        var session = await _context.ConversationSessions
            .FirstOrDefaultAsync(s => s.SessionId == sessionId);

        if (session != null)
        {
            _context.ConversationSessions.Remove(session);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Sessão removida: {SessionId}", sessionId);
        }
    }

    public async Task<IEnumerable<ConversationSession>> GetOldSessionsAsync(DateTime cutoffTime)
    {
        return await _context.ConversationSessions
            .Where(s => s.LastActivity < cutoffTime)
            .ToListAsync();
    }

    public async Task AddMessageAsync(string sessionId, ConversationMessage message)
    {
        // Verifica se a sessão existe
        var session = await _context.ConversationSessions
            .FirstOrDefaultAsync(s => s.SessionId == sessionId);
            
        if (session == null)
        {
            _logger.LogWarning("Sessão não encontrada para adicionar mensagem: {SessionId}", sessionId);
            return;
        }

        message.SessionId = sessionId;
        await _context.ConversationMessages.AddAsync(message);
        await _context.SaveChangesAsync();
        
        _logger.LogInformation("Mensagem adicionada à sessão {SessionId}", sessionId);
    }

    public async Task<IEnumerable<ConversationMessage>> GetMessagesAsync(string sessionId, int limit = 10)
    {
        return await _context.ConversationMessages
            .Where(m => m.SessionId == sessionId)
            .OrderByDescending(m => m.Timestamp)
            .Take(limit)
            .ToListAsync();
    }
} 