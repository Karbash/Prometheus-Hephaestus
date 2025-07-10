using Hephaestus.Domain.Entities;
using Hephaestus.Domain.Repositories;
using Hephaestus.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Hephaestus.Infrastructure.Repositories;

public class SalesRepository : ISalesRepository
{
    private readonly HephaestusDbContext _context;

    public SalesRepository(HephaestusDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<SalesLog>> GetSalesAsync(DateTime? startDate, DateTime? endDate, string? tenantId)
    {
        Console.WriteLine($"Buscando vendas - startDate: {startDate}, endDate: {endDate}, tenantId: {tenantId}");
        var query = _context.SalesLogs.AsQueryable();
        if (startDate.HasValue)
            query = query.Where(s => s.CreatedAt >= startDate.Value);
        if (endDate.HasValue)
            query = query.Where(s => s.CreatedAt <= endDate.Value);
        if (!string.IsNullOrEmpty(tenantId))
            query = query.Where(s => s.TenantId == tenantId);
        var sales = await query.ToListAsync();
        Console.WriteLine($"Vendas encontradas: {JsonSerializer.Serialize(sales)}");
        return sales;
    }
}