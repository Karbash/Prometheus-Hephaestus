using Hephaestus.Domain.Entities;
using Hephaestus.Domain.Repositories;
using Hephaestus.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Hephaestus.Infrastructure.Repositories;

public class AuditLogRepository : IAuditLogRepository
{
    private readonly HephaestusDbContext _context;

    public AuditLogRepository(HephaestusDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<AuditLog>> GetLogsAsync(string? adminId, DateTime? startDate, DateTime? endDate)
    {
        Console.WriteLine($"Buscando logs de auditoria - adminId: {adminId}, startDate: {startDate}, endDate: {endDate}");
        var query = _context.AuditLogs.AsQueryable();
        if (!string.IsNullOrEmpty(adminId))
            query = query.Where(l => l.AdminId == adminId);
        if (startDate.HasValue)
            query = query.Where(l => l.CreatedAt >= startDate.Value);
        if (endDate.HasValue)
            query = query.Where(l => l.CreatedAt <= endDate.Value);
        var logs = await query.ToListAsync();
        Console.WriteLine($"Logs encontrados: {JsonSerializer.Serialize(logs)}");
        return logs;
    }

    public async Task AddAsync(AuditLog auditLog)
    {
        Console.WriteLine($"Adicionando log de auditoria: {JsonSerializer.Serialize(auditLog)}");
        await _context.AuditLogs.AddAsync(auditLog);
        await _context.SaveChangesAsync();
    }
}