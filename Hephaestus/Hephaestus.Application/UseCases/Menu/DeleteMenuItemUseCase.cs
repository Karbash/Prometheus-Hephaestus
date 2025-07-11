using Hephaestus.Application.Interfaces.Menu;
using Hephaestus.Domain.Repositories;

namespace Hephaestus.Application.UseCases.Menu;

public class DeleteMenuItemUseCase : IDeleteMenuItemUseCase
{
    private readonly IMenuItemRepository _menuItemRepository;

    public DeleteMenuItemUseCase(IMenuItemRepository menuItemRepository)
    {
        _menuItemRepository = menuItemRepository;
    }

    public async Task ExecuteAsync(string id, string tenantId)
    {
        await _menuItemRepository.DeleteAsync(id, tenantId);
    }
}