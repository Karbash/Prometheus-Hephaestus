using Hephaestus.Domain.Entities;
using Hephaestus.Domain.Repositories;
using Hephaestus.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hephaestus.Infrastructure.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly HephaestusDbContext _context;

    public OrderRepository(HephaestusDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Order order)
    {
        await _context.Orders.AddAsync(order);
        await _context.SaveChangesAsync();
    }

    public async Task<Order?> GetByIdAsync(string id, string tenantId)
    {
        return await _context.Orders
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == id && o.TenantId == tenantId);
    }

    public async Task<IEnumerable<Order>> GetByTenantIdAsync(string tenantId, string? customerPhoneNumber, string? status)
    {
        var query = _context.Orders
            .AsNoTracking()
            .Where(o => o.TenantId == tenantId);

        if (!string.IsNullOrEmpty(customerPhoneNumber))
        {
            query = query.Where(o => o.CustomerPhoneNumber == customerPhoneNumber);
        }

        if (!string.IsNullOrEmpty(status))
        {
            query = query.Where(o => o.Status.ToString() == status);
        }

        return await query.ToListAsync();
    }

    public async Task UpdateAsync(Order order)
    {
        _context.Orders.Update(order);
        await _context.SaveChangesAsync();
    }
}