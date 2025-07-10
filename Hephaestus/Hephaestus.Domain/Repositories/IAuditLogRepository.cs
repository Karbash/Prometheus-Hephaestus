using Hephaestus.Domain.Entities;

namespace Hephaestus.Domain.Repositories;

public interface IAuditLogRepository
{
    Task<IEnumerable<AuditLog>> GetLogsAsync(string? userId, DateTime? startDate, DateTime? endDate);
    Task AddAsync(AuditLog auditLog);
}