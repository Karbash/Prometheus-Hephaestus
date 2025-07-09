using Hephaestus.Domain.Entities;
using Hephaestus.Domain.Repositories;

namespace Hephaestus.Infrastructure.Data;

public class AuditLogRepository : IAuditLogRepository
{
    private readonly HephaestusDbContext _context;

    public AuditLogRepository(HephaestusDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(AuditLog auditLog)
    {
        await _context.AuditLogs.AddAsync(auditLog);
        await _context.SaveChangesAsync();
    }
}