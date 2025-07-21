using Hephaestus.Domain.Entities;

namespace Hephaestus.Domain.Repositories;

public interface IConversationContextRepository
{
    Task<ConversationContext?> GetByPhoneNumberAsync(string phoneNumber);
    Task<ConversationContext> CreateAsync(ConversationContext context);
    Task<ConversationContext> UpdateAsync(ConversationContext context);
    Task DeleteAsync(string id);
    Task<List<ConversationContext>> GetInactiveContextsAsync(TimeSpan threshold);
} 