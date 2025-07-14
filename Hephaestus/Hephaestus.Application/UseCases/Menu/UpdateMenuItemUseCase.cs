using FluentValidation;
using Hephaestus.Application.DTOs.Request;
using Hephaestus.Application.Interfaces.Menu;
using Hephaestus.Domain.Repositories;
using Hephaestus.Application.Exceptions;
using Hephaestus.Application.Base;
using Microsoft.Extensions.Logging;
using Hephaestus.Application.Services;
using FluentValidation.Results;

namespace Hephaestus.Application.UseCases.Menu;

/// <summary>
/// Caso de uso para atualização de itens do cardápio.
/// </summary>
public class UpdateMenuItemUseCase : BaseUseCase, IUpdateMenuItemUseCase
{
    private readonly IMenuItemRepository _menuItemRepository;
    private readonly ITagRepository _tagRepository;
    private readonly IValidator<UpdateMenuItemRequest> _validator;

    /// <summary>
    /// Inicializa uma nova instância do <see cref="UpdateMenuItemUseCase"/>.
    /// </summary>
    /// <param name="menuItemRepository">Repositório de itens do cardápio.</param>
    /// <param name="tagRepository">Repositório de tags.</param>
    /// <param name="validator">Validador para a requisição.</param>
    /// <param name="logger">Logger.</param>
    /// <param name="exceptionHandler">Serviço de tratamento de exceções.</param>
    public UpdateMenuItemUseCase(
        IMenuItemRepository menuItemRepository,
        ITagRepository tagRepository,
        IValidator<UpdateMenuItemRequest> validator,
        ILogger<UpdateMenuItemUseCase> logger,
        IExceptionHandlerService exceptionHandler)
        : base(logger, exceptionHandler)
    {
        _menuItemRepository = menuItemRepository;
        _tagRepository = tagRepository;
        _validator = validator;
    }

    /// <summary>
    /// Executa a atualização de um item do cardápio.
    /// </summary>
    /// <param name="id">ID do item do cardápio.</param>
    /// <param name="request">Dados atualizados do item do cardápio.</param>
    /// <param name="tenantId">ID do tenant.</param>
    public async Task ExecuteAsync(string id, UpdateMenuItemRequest request, string tenantId)
    {
        await ExecuteWithExceptionHandlingAsync(async () =>
        {
            // Validação dos dados de entrada
            await ValidateRequestAsync(request, id);

            // Validação das regras de negócio
            await ValidateBusinessRulesAsync(request, tenantId);

            // Busca e validação da existência do item
            var menuItem = await GetAndValidateMenuItemAsync(id, tenantId);

            // Atualização do item do cardápio
            await UpdateMenuItemAsync(menuItem, request, id, tenantId);
        });
    }

    /// <summary>
    /// Valida os dados da requisição.
    /// </summary>
    /// <param name="request">Requisição a ser validada.</param>
    /// <param name="id">ID do item do cardápio.</param>
    private async Task ValidateRequestAsync(UpdateMenuItemRequest request, string id)
    {
        var validationResult = await _validator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            throw new Hephaestus.Application.Exceptions.ValidationException("Dados do item do cardápio inválidos", validationResult);
        }

        if (request.Id != id)
        {
            throw new Hephaestus.Application.Exceptions.ValidationException("ID no corpo da requisição deve corresponder ao ID na URL.", new ValidationResult());
        }
    }

    /// <summary>
    /// Valida as regras de negócio.
    /// </summary>
    /// <param name="request">Requisição com os dados.</param>
    /// <param name="tenantId">ID do tenant.</param>
    private async Task ValidateBusinessRulesAsync(UpdateMenuItemRequest request, string tenantId)
    {
        if (request.TagIds != null && request.TagIds.Any())
        {
            var isValid = await _menuItemRepository.ValidateTagIdsAsync(request.TagIds, tenantId);
            if (!isValid)
            {
                throw new BusinessRuleException("Um ou mais TagIds são inválidos para este tenant.", "TAG_VALIDATION_RULE");
            }
        }
    }

    /// <summary>
    /// Busca e valida a existência do item do cardápio.
    /// </summary>
    /// <param name="id">ID do item do cardápio.</param>
    /// <param name="tenantId">ID do tenant.</param>
    /// <returns>Item do cardápio encontrado.</returns>
    private async Task<Domain.Entities.MenuItem> GetAndValidateMenuItemAsync(string id, string tenantId)
    {
        var menuItem = await _menuItemRepository.GetByIdAsync(id, tenantId);
        EnsureEntityExists(menuItem, "MenuItem", id);
        return menuItem;
    }

    /// <summary>
    /// Atualiza o item do cardápio com os novos dados.
    /// </summary>
    /// <param name="menuItem">Item do cardápio a ser atualizado.</param>
    /// <param name="request">Dados atualizados.</param>
    /// <param name="id">ID do item do cardápio.</param>
    /// <param name="tenantId">ID do tenant.</param>
    private async Task UpdateMenuItemAsync(Domain.Entities.MenuItem menuItem, UpdateMenuItemRequest request, string id, string tenantId)
    {
        // Atualiza as propriedades do item
        menuItem.Name = request.Name ?? menuItem.Name;
        menuItem.Description = request.Description ?? menuItem.Description;
        menuItem.CategoryId = request.CategoryId ?? menuItem.CategoryId;
        menuItem.Price = request.Price ?? menuItem.Price;
        menuItem.IsAvailable = request.IsAvailable ?? menuItem.IsAvailable;
        menuItem.AvailableAdditionalIds = request.AvailableAdditionalIds ?? menuItem.AvailableAdditionalIds;
        menuItem.ImageUrl = request.ImageUrl ?? menuItem.ImageUrl;

        // Atualiza as tags se especificadas
        if (request.TagIds != null)
        {
            await _menuItemRepository.AddTagsAsync(id, request.TagIds, tenantId);
        }

        // Persiste as alterações
        await _menuItemRepository.UpdateAsync(menuItem);
    }
}