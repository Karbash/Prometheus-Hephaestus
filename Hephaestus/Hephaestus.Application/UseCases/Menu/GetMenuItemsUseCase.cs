using Hephaestus.Application.DTOs.Response;
using Hephaestus.Application.Interfaces.Menu;
using Hephaestus.Domain.Repositories;

namespace Hephaestus.Application.UseCases.Menu;

public class GetMenuItemsUseCase : IGetMenuItemsUseCase
{
    private readonly IMenuItemRepository _menuItemRepository;

    public GetMenuItemsUseCase(IMenuItemRepository menuItemRepository)
    {
        _menuItemRepository = menuItemRepository;
    }

    public async Task<IEnumerable<MenuItemResponse>> ExecuteAsync(string tenantId)
    {
        var menuItems = await _menuItemRepository.GetByTenantIdAsync(tenantId);
        var response = menuItems.Select(m => new MenuItemResponse
        {
            Id = m.Id,
            TenantId = m.TenantId,
            Name = m.Name,
            Description = m.Description,
            CategoryId = m.CategoryId,
            Price = m.Price,
            IsAvailable = m.IsAvailable,
            Tags = m.Tags,
            AvailableAdditionalIds = m.AvailableAdditionalIds,
            ImageUrl = m.ImageUrl
        }).ToList();

        return response;
    }
}