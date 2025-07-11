using Hephaestus.Domain.Entities;

namespace Hephaestus.Domain.Repositories;

/// <summary>
/// Interface para o repositório de itens do cardápio.
/// </summary>
public interface IMenuItemRepository
{
    /// <summary>
    /// Adiciona um novo item do cardápio.
    /// </summary>
    Task AddAsync(MenuItem menuItem);

    /// <summary>
    /// Lista itens do cardápio de um tenant.
    /// </summary>
    Task<IEnumerable<MenuItem>> GetByTenantIdAsync(string tenantId);

    /// <summary>
    /// Obtém um item do cardápio por ID e tenant.
    /// </summary>
    Task<MenuItem?> GetByIdAsync(string id, string tenantId);

    /// <summary>
    /// Atualiza um item do cardápio.
    /// </summary>
    Task UpdateAsync(MenuItem menuItem);

    /// <summary>
    /// Remove um item do cardápio.
    /// </summary>
    Task DeleteAsync(string id, string tenantId);
}