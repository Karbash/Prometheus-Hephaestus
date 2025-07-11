using FluentValidation;
using Hephaestus.Application.DTOs.Request;
using Hephaestus.Application.Interfaces.Menu;
using Hephaestus.Domain.Entities;
using Hephaestus.Domain.Repositories;

namespace Hephaestus.Application.UseCases.Menu;

public class CreateMenuItemUseCase : ICreateMenuItemUseCase
{
    private readonly IMenuItemRepository _menuItemRepository;
    private readonly IValidator<CreateMenuItemRequest> _validator;

    public CreateMenuItemUseCase(IMenuItemRepository menuItemRepository, IValidator<CreateMenuItemRequest> validator)
    {
        _menuItemRepository = menuItemRepository;
        _validator = validator;
    }

    public async Task<string> ExecuteAsync(CreateMenuItemRequest request, string tenantId)
    {
        var validationResult = await _validator.ValidateAsync(request);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var menuItem = new MenuItem
        {
            TenantId = tenantId,
            Name = request.Name,
            Description = request.Description,
            CategoryId = request.CategoryId,
            Price = request.Price,
            IsAvailable = request.IsAvailable,
            Tags = request.Tags,
            AvailableAdditionalIds = request.AvailableAdditionalIds,
            ImageUrl = request.ImageUrl
        };

        await _menuItemRepository.AddAsync(menuItem);
        return menuItem.Id;
    }
}