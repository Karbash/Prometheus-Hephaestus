using Hephaestus.Domain.Entities;
using Hephaestus.Domain.Repositories;
using Hephaestus.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Hephaestus.Infrastructure.Repositories;

public class AddressRepository : IAddressRepository
{
    private readonly HephaestusDbContext _context;

    public AddressRepository(HephaestusDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Address address)
    {
        await _context.Addresses.AddAsync(address);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Address address)
    {
        _context.Addresses.Update(address);
        await _context.SaveChangesAsync();
    }

    public async Task<Address?> GetByIdAsync(string id)
    {
        return await _context.Addresses.AsNoTracking().FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<IEnumerable<Address>> GetByEntityAsync(string entityId, string entityType)
    {
        return await _context.Addresses.AsNoTracking()
            .Where(a => a.EntityId == entityId && a.EntityType == entityType)
            .ToListAsync();
    }

    public async Task DeleteAsync(string id)
    {
        var address = await _context.Addresses.FirstOrDefaultAsync(a => a.Id == id);
        if (address != null)
        {
            _context.Addresses.Remove(address);
            await _context.SaveChangesAsync();
        }
    }
} 