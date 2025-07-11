using Hephaestus.Domain.Entities;
using Hephaestus.Domain.Repositories;
using Hephaestus.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Hephaestus.Infrastructure.Repositories;

public class MenuItemRepository : IMenuItemRepository
{
    private readonly HephaestusDbContext _context;

    public MenuItemRepository(HephaestusDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(MenuItem menuItem)
    {
        Console.WriteLine($"Adicionando item do cardápio: {JsonSerializer.Serialize(menuItem)}");
        try
        {
            _context.MenuItems.Add(menuItem);
            var changes = await _context.SaveChangesAsync();
            Console.WriteLine($"Alterações salvas: {changes}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao adicionar item do cardápio: {ex.Message}");
            throw;
        }
    }

    public async Task<IEnumerable<MenuItem>> GetByTenantIdAsync(string tenantId)
    {
        Console.WriteLine($"Buscando itens do cardápio para TenantId: {tenantId}");
        try
        {
            var menuItems = await _context.MenuItems
                .Where(m => m.TenantId == tenantId)
                .ToListAsync();
            Console.WriteLine($"Itens encontrados: {JsonSerializer.Serialize(menuItems)}");
            return menuItems;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao buscar itens do cardápio: {ex.Message}");
            throw;
        }
    }

    public async Task<MenuItem?> GetByIdAsync(string id, string tenantId)
    {
        Console.WriteLine($"Buscando item do cardápio por ID: {id}, TenantId: {tenantId}");
        try
        {
            var menuItem = await _context.MenuItems
                .FirstOrDefaultAsync(m => m.Id == id && m.TenantId == tenantId);
            Console.WriteLine($"Item encontrado: {JsonSerializer.Serialize(menuItem)}");
            return menuItem;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao buscar item do cardápio: {ex.Message}");
            throw;
        }
    }

    public async Task UpdateAsync(MenuItem menuItem)
    {
        Console.WriteLine($"Atualizando item do cardápio: {JsonSerializer.Serialize(menuItem)}");
        try
        {
            _context.MenuItems.Update(menuItem);
            var changes = await _context.SaveChangesAsync();
            Console.WriteLine($"Alterações salvas: {changes}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao atualizar item do cardápio: {ex.Message}");
            throw;
        }
    }

    public async Task DeleteAsync(string id, string tenantId)
    {
        Console.WriteLine($"Removendo item do cardápio por ID: {id}, TenantId: {tenantId}");
        try
        {
            var menuItem = await _context.MenuItems
                .FirstOrDefaultAsync(m => m.Id == id && m.TenantId == tenantId);
            if (menuItem == null)
                throw new KeyNotFoundException("Item do cardápio não encontrado.");

            _context.MenuItems.Remove(menuItem);
            var changes = await _context.SaveChangesAsync();
            Console.WriteLine($"Alterações salvas: {changes}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao remover item do cardápio: {ex.Message}");
            throw;
        }
    }
}