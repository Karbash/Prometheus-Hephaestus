using Hephaestus.Domain.Entities;
using Hephaestus.Domain.Repositories;
using Hephaestus.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Hephaestus.Infrastructure.Repositories;

public class TagRepository : ITagRepository
{
    private readonly HephaestusDbContext _context;

    public TagRepository(HephaestusDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Tag tag)
    {
        await _context.Tags.AddAsync(tag);
        await _context.SaveChangesAsync();
    }

    public async Task<Tag?> GetByNameAsync(string name, string tenantId)
    {
        return await _context.Tags
            .FirstOrDefaultAsync(t => t.Name == name && t.TenantId == tenantId);
    }

    public async Task<IEnumerable<Tag>> GetByTenantIdAsync(string tenantId)
    {
        return await _context.Tags
            .Where(t => t.TenantId == tenantId)
            .ToListAsync();
    }
}