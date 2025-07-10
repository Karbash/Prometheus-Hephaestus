using Hephaestus.Domain.Entities;

namespace Hephaestus.Domain.Repositories;

public interface IAuditLogRepository
{
    Task<IEnumerable<AuditLog>> GetLogsAsync(string? adminId, DateTime? startDate, DateTime? endDate);
    Task AddAsync(AuditLog auditLog);
}