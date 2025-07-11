using FluentValidation;
using Hephaestus.Application.DTOs.Request;
using Hephaestus.Application.Interfaces.Menu;
using Hephaestus.Domain.Repositories;
using System;
using System.Threading.Tasks;

namespace Hephaestus.Application.UseCases.Menu;

public class UpdateMenuItemUseCase : IUpdateMenuItemUseCase
{
    private readonly IMenuItemRepository _menuItemRepository;
    private readonly IValidator<UpdateMenuItemRequest> _validator;

    public UpdateMenuItemUseCase(IMenuItemRepository menuItemRepository, IValidator<UpdateMenuItemRequest> validator)
    {
        _menuItemRepository = menuItemRepository;
        _validator = validator;
    }

    public async Task ExecuteAsync(string id, UpdateMenuItemRequest request, string tenantId)
    {
        var validationResult = await _validator.ValidateAsync(request);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        if (request.Id != id)
            throw new ArgumentException("ID no corpo da requisição deve corresponder ao ID na URL.");

        var menuItem = await _menuItemRepository.GetByIdAsync(id, tenantId);
        if (menuItem == null)
            throw new KeyNotFoundException("Item do cardápio não encontrado.");

        menuItem.Name = request.Name ?? menuItem.Name;
        menuItem.Description = request.Description ?? menuItem.Description;
        menuItem.CategoryId = request.CategoryId ?? menuItem.CategoryId;
        menuItem.Price = request.Price ?? menuItem.Price;
        menuItem.IsAvailable = request.IsAvailable ?? menuItem.IsAvailable;
        menuItem.Tags = request.Tags ?? menuItem.Tags;
        menuItem.AvailableAdditionalIds = request.AvailableAdditionalIds ?? menuItem.AvailableAdditionalIds;
        menuItem.ImageUrl = request.ImageUrl ?? menuItem.ImageUrl;

        await _menuItemRepository.UpdateAsync(menuItem);
    }
}