using Hephaestus.Domain.Entities;
using Hephaestus.Domain.Repositories;
using Hephaestus.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace Hephaestus.Infrastructure.Repositories;

public class SalesRepository : ISalesRepository
{
    private readonly HephaestusDbContext _context;
    private readonly ILogger<SalesRepository> _logger;

    public SalesRepository(HephaestusDbContext context, ILogger<SalesRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IEnumerable<SalesLog>> GetSalesAsync(DateTime? startDate, DateTime? endDate, string? tenantId)
    {
        _logger.LogInformation("Buscando vendas - startDate: {StartDate}, endDate: {EndDate}, tenantId: {TenantId}", startDate, endDate, tenantId);
        var query = _context.SalesLogs.AsQueryable();
        if (startDate.HasValue)
            query = query.Where(s => s.CreatedAt >= startDate.Value);
        if (endDate.HasValue)
            query = query.Where(s => s.CreatedAt <= endDate.Value);
        if (!string.IsNullOrEmpty(tenantId))
            query = query.Where(s => s.TenantId == tenantId);
        var sales = await query.AsNoTracking().ToListAsync();
        _logger.LogInformation("Vendas encontradas: {@Sales}", sales);
        return sales;
    }

    public async Task AddAsync(SalesLog salesLog)
    {
        await _context.SalesLogs.AddAsync(salesLog);
        await _context.SaveChangesAsync();
    }
}