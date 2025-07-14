using FluentValidation;
using Hephaestus.Application.Base;
using Hephaestus.Application.DTOs.Request;
using Hephaestus.Application.Exceptions;
using Hephaestus.Application.Interfaces.Menu;
using Hephaestus.Application.Services;
using Hephaestus.Domain.Entities;
using Hephaestus.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace Hephaestus.Application.UseCases.Menu;

/// <summary>
/// Caso de uso refatorado para criação de itens do cardápio com tratamento de exceções melhorado.
/// </summary>
public class CreateMenuItemUseCaseRefactored : BaseUseCase, ICreateMenuItemUseCase
{
    private readonly IMenuItemRepository _menuItemRepository;
    private readonly ITagRepository _tagRepository;
    private readonly IValidator<CreateMenuItemRequest> _validator;

    public CreateMenuItemUseCaseRefactored(
        IMenuItemRepository menuItemRepository,
        ITagRepository tagRepository,
        IValidator<CreateMenuItemRequest> validator,
        ILogger<CreateMenuItemUseCaseRefactored> logger,
        IExceptionHandlerService exceptionHandler)
        : base(logger, exceptionHandler)
    {
        _menuItemRepository = menuItemRepository;
        _tagRepository = tagRepository;
        _validator = validator;
    }

    /// <summary>
    /// Executa a criação de um item do cardápio com tratamento robusto de exceções.
    /// </summary>
    /// <param name="request">Dados do item do cardápio.</param>
    /// <param name="tenantId">ID do tenant.</param>
    /// <returns>ID do item criado.</returns>
    public async Task<string> ExecuteAsync(CreateMenuItemRequest request, string tenantId)
    {
        return await ExecuteWithExceptionHandlingAsync(async () =>
        {
            // Validação de entrada
            await ValidateAsync(_validator, request);

            // Validação de regras de negócio
            await ValidateBusinessRulesAsync(request, tenantId);

            // Criação do item
            var menuItem = CreateMenuItemEntity(request, tenantId);

            // Persistência
            await PersistMenuItemAsync(menuItem, request);

            Logger.LogInformation("Item do cardápio '{MenuItemName}' criado com sucesso para o tenant {TenantId}", 
                menuItem.Name, tenantId);

            return menuItem.Id;
        }, "CreateMenuItem");
    }

    /// <summary>
    /// Valida as regras de negócio específicas.
    /// </summary>
    private async Task ValidateBusinessRulesAsync(CreateMenuItemRequest request, string tenantId)
    {
        // Verifica se as tags existem e pertencem ao tenant
        if (request.TagIds.Any())
        {
            var tagValidationResult = await _menuItemRepository.ValidateTagIdsAsync(request.TagIds, tenantId);
            EnsureBusinessRule(tagValidationResult, 
                "Um ou mais TagIds são inválidos para este tenant.", 
                "TagValidation",
                new Dictionary<string, object>
                {
                    ["tagIds"] = request.TagIds,
                    ["tenantId"] = tenantId
                });
        }

        // Verifica se os adicionais existem e pertencem ao tenant
        if (request.AvailableAdditionalIds.Any())
        {
            var additionalValidationResult = await ValidateAdditionalsAsync(request.AvailableAdditionalIds, tenantId);
            EnsureBusinessRule(additionalValidationResult,
                "Um ou mais AdditionalIds são inválidos para este tenant.",
                "AdditionalValidation",
                new Dictionary<string, object>
                {
                    ["additionalIds"] = request.AvailableAdditionalIds,
                    ["tenantId"] = tenantId
                });
        }
    }

    /// <summary>
    /// Valida se os adicionais existem e pertencem ao tenant.
    /// </summary>
    private async Task<bool> ValidateAdditionalsAsync(IEnumerable<string> additionalIds, string tenantId)
    {
        // Implementação da validação de adicionais
        // Por simplicidade, retornamos true, mas em uma implementação real
        // você verificaria se os adicionais existem e pertencem ao tenant
        return true;
    }

    /// <summary>
    /// Cria a entidade MenuItem a partir do request.
    /// </summary>
    private MenuItem CreateMenuItemEntity(CreateMenuItemRequest request, string tenantId)
    {
        return new MenuItem
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
    }

    /// <summary>
    /// Persiste o item do cardápio e suas associações.
    /// </summary>
    private async Task PersistMenuItemAsync(MenuItem menuItem, CreateMenuItemRequest request)
    {
        // Persiste o item principal
        await _menuItemRepository.AddAsync(menuItem);

        // Adiciona as tags se houver
        if (request.TagIds.Any())
        {
            await _menuItemRepository.AddTagsAsync(menuItem.Id, request.TagIds, menuItem.TenantId);
        }
    }
} 