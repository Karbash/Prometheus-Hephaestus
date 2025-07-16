using FluentValidation;
using Hephaestus.Domain.DTOs.Request;
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
/// Caso de uso para atualiza��o de itens do card�pio.
/// </summary>
public class UpdateMenuItemUseCase : BaseUseCase, IUpdateMenuItemUseCase
{
    private readonly IMenuItemRepository _menuItemRepository;
    private readonly ITagRepository _tagRepository;
    private readonly IValidator<UpdateMenuItemRequest> _validator;
    private readonly ILoggedUserService _loggedUserService;

    /// <summary>
    /// Inicializa uma nova inst�ncia do <see cref="UpdateMenuItemUseCase"/>.
    /// </summary>
    /// <param name="menuItemRepository">Reposit�rio de itens do card�pio.</param>
    /// <param name="tagRepository">Reposit�rio de tags.</param>
    /// <param name="validator">Validador para a requisi��o.</param>
    /// <param name="loggedUserService">Servi�o para obter informa��es do usu�rio logado.</param>
    /// <param name="logger">Logger.</param>
    /// <param name="exceptionHandler">Servi�o de tratamento de exce��es.</param>
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
    /// Executa a atualiza��o de um item do card�pio.
    /// </summary>
    /// <param name="id">ID do item do card�pio.</param>
    /// <param name="request">Dados atualizados do item do card�pio.</param>
    /// <param name="user">Usu�rio autenticado.</param>
    public async Task ExecuteAsync(string id, UpdateMenuItemRequest request, ClaimsPrincipal user)
    {
        await ExecuteWithExceptionHandlingAsync(async () =>
        {
            var tenantId = _loggedUserService.GetTenantId(user);
            
            // Valida��o dos dados de entrada
            await _validator.ValidateAndThrowAsync(request);

            // Valida��o das regras de neg�cio
            await ValidateBusinessRulesAsync(request, tenantId);

            // Busca e valida��o do item
            var menuItem = await _menuItemRepository.GetByIdAsync(id, tenantId);
            EnsureResourceExists(menuItem, "MenuItem", id);

            // Atualiza��o dos dados
            await UpdateMenuItemEntityAsync(menuItem!, request);
        });
    }

    /// <summary>
    /// Valida as regras de neg�cio.
    /// </summary>
    /// <param name="request">Requisi��o com os dados.</param>
    /// <param name="tenantId">ID do tenant.</param>
    private async Task ValidateBusinessRulesAsync(UpdateMenuItemRequest request, string tenantId)
    {
        if (request.TagIds != null && request.TagIds.Any())
        {
            var isValid = await _menuItemRepository.ValidateTagIdsAsync(request.TagIds, tenantId);
            if (!isValid)
            {
                throw new BusinessRuleException("Um ou mais TagIds s�o inv�lidos para este tenant.", "TAG_VALIDATION_RULE");
            }
        }
    }

    /// <summary>
    /// Atualiza o item do card�pio com os novos dados.
    /// </summary>
    /// <param name="menuItem">Item do card�pio a ser atualizado.</param>
    /// <param name="request">Dados atualizados.</param>
    /// <param name="id">ID do item do card�pio.</param>
    /// <param name="tenantId">ID do tenant.</param>
    private async Task UpdateMenuItemEntityAsync(Domain.Entities.MenuItem menuItem, UpdateMenuItemRequest request)
    {
        // Atualiza as propriedades do item
        menuItem.Name = request.Name ?? menuItem.Name;
        menuItem.Description = request.Description ?? menuItem.Description;
        menuItem.CategoryId = request.CategoryId ?? menuItem.CategoryId;
        menuItem.Price = request.Price ?? menuItem.Price;
        menuItem.IsAvailable = request.IsAvailable ?? menuItem.IsAvailable;
        // AvailableAdditionalIds foi removido da entidade
        menuItem.ImageUrl = request.ImageUrl ?? menuItem.ImageUrl;

        // Atualiza as tags se especificadas
        if (request.TagIds != null)
        {
            await _menuItemRepository.AddTagsAsync(menuItem.Id, request.TagIds, menuItem.TenantId);
        }

        // Persiste as altera��es
        await _menuItemRepository.UpdateAsync(menuItem);
    }
}
