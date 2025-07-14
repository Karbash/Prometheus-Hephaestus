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
        if (tag == null)
            throw new ArgumentNullException(nameof(tag));

        await _context.Tags.AddAsync(tag);
        await _context.SaveChangesAsync();
    }

    public async Task<Tag?> GetByNameAsync(string name, string tenantId)
    {
        if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(tenantId))
            throw new ArgumentException("Name e TenantId são obrigatórios.");

        return await _context.Tags
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Name == name && t.TenantId == tenantId);
    }

    public async Task<IEnumerable<Tag>> GetByTenantIdAsync(string tenantId)
    {
        if (string.IsNullOrEmpty(tenantId))
            throw new ArgumentException("TenantId é obrigatório.");

        return await _context.Tags
            .AsNoTracking()
            .Where(t => t.TenantId == tenantId)
            .ToListAsync();
    }

    public async Task<Tag?> GetByIdAsync(string id, string tenantId)
    {
        if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(tenantId))
            throw new ArgumentException("Id e TenantId são obrigatórios.");

        return await _context.Tags
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == id && t.TenantId == tenantId);
    }

    public async Task DeleteAsync(string id, string tenantId)
    {
        if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(tenantId))
            throw new ArgumentException("Id e TenantId são obrigatórios.");

        var tag = await _context.Tags
            .FirstOrDefaultAsync(t => t.Id == id && t.TenantId == tenantId);

        if (tag == null)
            return; // Não lança exceção, deixa o UseCase tratar

        var hasMenuItemTags = await _context.MenuItemTags
            .AnyAsync(mt => mt.TagId == id); // Corrigido: Removido TenantId

        if (hasMenuItemTags)
            throw new InvalidOperationException("Não é possível excluir a tag pois ela está associada a itens do cardápio.");

        _context.Tags.Remove(tag);
        await _context.SaveChangesAsync();
    }
}