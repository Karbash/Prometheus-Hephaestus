using Hephaestus.Application.DTOs.Response;
using Hephaestus.Application.Interfaces.Menu;
using Hephaestus.Domain.Repositories;

namespace Hephaestus.Application.UseCases.Menu;

public class GetMenuItemByIdUseCase : IGetMenuItemByIdUseCase
{
    private readonly IMenuItemRepository _menuItemRepository;

    public GetMenuItemByIdUseCase(IMenuItemRepository menuItemRepository)
    {
        _menuItemRepository = menuItemRepository;
    }

    public async Task<MenuItemResponse> ExecuteAsync(string id, string tenantId)
    {
        var menuItem = await _menuItemRepository.GetByIdAsync(id, tenantId);
        if (menuItem == null)
            throw new KeyNotFoundException("Item do cardápio não encontrado.");

        return new MenuItemResponse
        {
            Id = menuItem.Id,
            TenantId = menuItem.TenantId,
            Name = menuItem.Name,
            Description = menuItem.Description,
            CategoryId = menuItem.CategoryId,
            Price = menuItem.Price,
            IsAvailable = menuItem.IsAvailable,
            Tags = menuItem.MenuItemTags.Select(mt => mt.Tag.Name).ToList(),
            AvailableAdditionalIds = menuItem.AvailableAdditionalIds,
            ImageUrl = menuItem.ImageUrl
        };
    }
}