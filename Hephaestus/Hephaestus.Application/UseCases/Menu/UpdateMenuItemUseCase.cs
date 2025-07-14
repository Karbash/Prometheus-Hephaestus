using FluentValidation;
using Hephaestus.Application.DTOs.Request;
using Hephaestus.Application.Interfaces.Menu;
using Hephaestus.Domain.Repositories;
using Hephaestus.Application.Exceptions;
using Hephaestus.Application.Base;
using Microsoft.Extensions.Logging;
using Hephaestus.Application.Services;
using Hephaestus.Domain.Services;
using FluentValidation.Results;
using System.Security.Claims;

namespace Hephaestus.Application.UseCases.Menu;

/// <summary>
/// Caso de uso para atualização de itens do cardápio.
/// </summary>
public class UpdateMenuItemUseCase : BaseUseCase, IUpdateMenuItemUseCase
{
    private readonly IMenuItemRepository _menuItemRepository;
    private readonly ITagRepository _tagRepository;
    private readonly IValidator<UpdateMenuItemRequest> _validator;
    private readonly ILoggedUserService _loggedUserService;

    /// <summary>
    /// Inicializa uma nova instância do <see cref="UpdateMenuItemUseCase"/>.
    /// </summary>
    /// <param name="menuItemRepository">Repositório de itens do cardápio.</param>
    /// <param name="tagRepository">Repositório de tags.</param>
    /// <param name="validator">Validador para a requisição.</param>
    /// <param name="loggedUserService">Serviço para obter informações do usuário logado.</param>
    /// <param name="logger">Logger.</param>
    /// <param name="exceptionHandler">Serviço de tratamento de exceções.</param>
    public UpdateMenuItemUseCase(
        IMenuItemRepository menuItemRepository,
        ITagRepository tagRepository,
        IValidator<UpdateMenuItemRequest> validator,
        ILoggedUserService loggedUserService,
        ILogger<UpdateMenuItemUseCase> logger,
        IExceptionHandlerService exceptionHandler)
        : base(logger, exceptionHandler)
    {
        _menuItemRepository = menuItemRepository;
        _tagRepository = tagRepository;
        _validator = validator;
        _loggedUserService = loggedUserService;
    }

    /// <summary>
    /// Executa a atualização de um item do cardápio.
    /// </summary>
    /// <param name="id">ID do item do cardápio.</param>
    /// <param name="request">Dados atualizados do item do cardápio.</param>
    /// <param name="user">Usuário autenticado.</param>
    public async Task ExecuteAsync(string id, UpdateMenuItemRequest request, ClaimsPrincipal user)
    {
        await ExecuteWithExceptionHandlingAsync(async () =>
        {
            var tenantId = _loggedUserService.GetTenantId(user);
            
            // Validação dos dados de entrada
            await _validator.ValidateAndThrowAsync(request);

            // Validação das regras de negócio
            await ValidateBusinessRulesAsync(request, tenantId);

            // Busca e validação do item
            var menuItem = await _menuItemRepository.GetByIdAsync(id, tenantId);
            EnsureResourceExists(menuItem, "MenuItem", id);

            // Atualização dos dados
            await UpdateMenuItemEntityAsync(menuItem!, request);
        });
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
    /// Atualiza o item do cardápio com os novos dados.
    /// </summary>
    /// <param name="menuItem">Item do cardápio a ser atualizado.</param>
    /// <param name="request">Dados atualizados.</param>
    /// <param name="id">ID do item do cardápio.</param>
    /// <param name="tenantId">ID do tenant.</param>
    private async Task UpdateMenuItemEntityAsync(Domain.Entities.MenuItem menuItem, UpdateMenuItemRequest request)
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
            await _menuItemRepository.AddTagsAsync(menuItem.Id, request.TagIds, menuItem.TenantId);
        }

        // Persiste as alterações
        await _menuItemRepository.UpdateAsync(menuItem);
    }
}