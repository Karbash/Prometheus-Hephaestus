using FluentValidation;
using Hephaestus.Application.DTOs.Request;
using Hephaestus.Application.Interfaces.Menu;
using Hephaestus.Domain.Entities;
using Hephaestus.Domain.Repositories;

namespace Hephaestus.Application.UseCases.Menu;

public class UpdateMenuItemUseCase : IUpdateMenuItemUseCase
{
    private readonly IMenuItemRepository _menuItemRepository;
    private readonly ITagRepository _tagRepository;
    private readonly IValidator<UpdateMenuItemRequest> _validator;

    public UpdateMenuItemUseCase(
        IMenuItemRepository menuItemRepository,
        ITagRepository tagRepository,
        IValidator<UpdateMenuItemRequest> validator)
    {
        _menuItemRepository = menuItemRepository;
        _tagRepository = tagRepository;
        _validator = validator;
    }

    public async Task ExecuteAsync(string id, UpdateMenuItemRequest request, string tenantId)
    {
        var validationResult = await _validator.ValidateAsync(request);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        if (request.Id != id)
            throw new ArgumentException("ID no corpo da requisição deve corresponder ao ID na URL.");

        if (request.TagIds != null && request.TagIds.Any() && !await _menuItemRepository.ValidateTagIdsAsync(request.TagIds, tenantId))
            throw new InvalidOperationException("Um ou mais TagIds são inválidos para este tenant.");

        var menuItem = await _menuItemRepository.GetByIdAsync(id, tenantId);
        if (menuItem == null)
            throw new KeyNotFoundException("Item do cardápio não encontrado.");

        menuItem.Name = request.Name ?? menuItem.Name;
        menuItem.Description = request.Description ?? menuItem.Description;
        menuItem.CategoryId = request.CategoryId ?? menuItem.CategoryId;
        menuItem.Price = request.Price ?? menuItem.Price;
        menuItem.IsAvailable = request.IsAvailable ?? menuItem.IsAvailable;
        menuItem.AvailableAdditionalIds = request.AvailableAdditionalIds ?? menuItem.AvailableAdditionalIds;
        menuItem.ImageUrl = request.ImageUrl ?? menuItem.ImageUrl;

        if (request.TagIds != null)
        {
            await _menuItemRepository.AddTagsAsync(id, request.TagIds, tenantId);
        }

        await _menuItemRepository.UpdateAsync(menuItem);
    }
}