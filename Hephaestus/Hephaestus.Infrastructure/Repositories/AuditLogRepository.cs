using Hephaestus.Domain.Entities;
using Hephaestus.Domain.Repositories;
using Hephaestus.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Hephaestus.Infrastructure.Repositories;

public class AuditLogRepository : IAuditLogRepository
{
    private readonly HephaestusDbContext _context;

    public AuditLogRepository(HephaestusDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(AuditLog auditLog)
    {
        if (auditLog == null)
            throw new ArgumentNullException(nameof(auditLog));

        await _context.AuditLogs.AddAsync(auditLog);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<AuditLog>> GetLogsAsync(string? userId, DateTime? startDate, DateTime? endDate)
    {
        var query = _context.AuditLogs.AsNoTracking();

        if (!string.IsNullOrEmpty(userId))
            query = query.Where(l => l.UserId == userId);

        if (startDate.HasValue)
            query = query.Where(l => l.CreatedAt >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(l => l.CreatedAt <= endDate.Value);

        return await query.ToListAsync();
    }
}