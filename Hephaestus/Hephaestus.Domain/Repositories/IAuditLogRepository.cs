using Hephaestus.Domain.Entities;

namespace Hephaestus.Domain.Repositories
{
    public interface IAuditLogRepository
    {
        Task AddAsync(AuditLog auditLog);
    }
}
