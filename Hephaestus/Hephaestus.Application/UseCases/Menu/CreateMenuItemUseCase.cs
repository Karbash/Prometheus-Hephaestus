using FluentValidation;
using Hephaestus.Application.DTOs.Request;
using Hephaestus.Application.Interfaces.Menu;
using Hephaestus.Domain.Entities;
using Hephaestus.Domain.Repositories;

namespace Hephaestus.Application.UseCases.Menu;

public class CreateMenuItemUseCase : ICreateMenuItemUseCase
{
    private readonly IMenuItemRepository _menuItemRepository;
    private readonly ITagRepository _tagRepository;
    private readonly IValidator<CreateMenuItemRequest> _validator;

    public CreateMenuItemUseCase(
        IMenuItemRepository menuItemRepository,
        ITagRepository tagRepository,
        IValidator<CreateMenuItemRequest> validator)
    {
        _menuItemRepository = menuItemRepository;
        _tagRepository = tagRepository;
        _validator = validator;
    }

    public async Task<string> ExecuteAsync(CreateMenuItemRequest request, string tenantId)
    {
        var validationResult = await _validator.ValidateAsync(request);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        if (request.TagIds.Any() && !await _menuItemRepository.ValidateTagIdsAsync(request.TagIds, tenantId))
            throw new InvalidOperationException("Um ou mais TagIds são inválidos para este tenant.");

        var menuItem = new MenuItem
        {
            Id = Guid.NewGuid().ToString(),
            TenantId = tenantId,
            Name = request.Name,
            Description = request.Description,
            CategoryId = request.CategoryId,
            Price = request.Price,
            IsAvailable = request.IsAvailable,
            AvailableAdditionalIds = request.AvailableAdditionalIds,
            ImageUrl = request.ImageUrl
        };

        await _menuItemRepository.AddAsync(menuItem);
        if (request.TagIds.Any())
        {
            await _menuItemRepository.AddTagsAsync(menuItem.Id, request.TagIds, tenantId);
        }

        return menuItem.Id;
    }
}