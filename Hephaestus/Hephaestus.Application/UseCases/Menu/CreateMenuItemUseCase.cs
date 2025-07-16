using FluentValidation;
using Hephaestus.Domain.DTOs.Request;
using Hephaestus.Application.Interfaces.Menu;
using Hephaestus.Domain.Entities;
using Hephaestus.Domain.Repositories;
using Hephaestus.Application.Exceptions;
using Hephaestus.Application.Base;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;
using Hephaestus.Application.Services;
using Hephaestus.Domain.Services;
using System.Security.Claims;

namespace Hephaestus.Application.UseCases.Menu;

/// <summary>
/// Caso de uso para cria��o de itens do card�pio.
/// </summary>
public class CreateMenuItemUseCase : BaseUseCase, ICreateMenuItemUseCase
{
    private readonly IMenuItemRepository _menuItemRepository;
    private readonly ITagRepository _tagRepository;
    private readonly IValidator<CreateMenuItemRequest> _validator;
    private readonly ILoggedUserService _loggedUserService;

    /// <summary>
    /// Inicializa uma nova inst�ncia do <see cref="CreateMenuItemUseCase"/>.
    /// </summary>
    /// <param name="menuItemRepository">Reposit�rio de itens do card�pio.</param>
    /// <param name="tagRepository">Reposit�rio de tags.</param>
    /// <param name="validator">Validador para a requisi��o.</param>
    /// <param name="loggedUserService">Servi�o para obter informa��es do usu�rio logado.</param>
    /// <param name="logger">Logger.</param>
    /// <param name="exceptionHandler">Servi�o de tratamento de exce��es.</param>
    public CreateMenuItemUseCase(
        IMenuItemRepository menuItemRepository,
        ITagRepository tagRepository,
        IValidator<CreateMenuItemRequest> validator,
        ILoggedUserService loggedUserService,
        ILogger<CreateMenuItemUseCase> logger,
        IExceptionHandlerService exceptionHandler)
        : base(logger, exceptionHandler)
    {
        _menuItemRepository = menuItemRepository;
        _tagRepository = tagRepository;
        _validator = validator;
        _loggedUserService = loggedUserService;
    }

    /// <summary>
    /// Executa a cria��o de um item do card�pio.
    /// </summary>
    /// <param name="request">Dados do item do card�pio a ser criado.</param>
    /// <param name="user">Usu�rio autenticado.</param>
    /// <returns>ID do item criado.</returns>
    public async Task<string> ExecuteAsync(CreateMenuItemRequest request, ClaimsPrincipal user)
    {
        return await ExecuteWithExceptionHandlingAsync(async () =>
        {
            var tenantId = _loggedUserService.GetTenantId(user);
            
            // Valida��o dos dados de entrada
            await _validator.ValidateAndThrowAsync(request);

            // Valida��o das regras de neg�cio
            await ValidateBusinessRulesAsync(request, tenantId);

            // Cria��o do item do card�pio
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

            // Adiciona tags se especificadas
            if (request.TagIds.Any())
            {
                await _menuItemRepository.AddTagsAsync(menuItem.Id, request.TagIds, tenantId);
            }

            return menuItem.Id;
        });
    }

    /// <summary>
    /// Valida as regras de neg�cio.
    /// </summary>
    /// <param name="request">Requisi��o com os dados.</param>
    /// <param name="tenantId">ID do tenant.</param>
    private async Task ValidateBusinessRulesAsync(CreateMenuItemRequest request, string tenantId)
    {
        // Valida se as tags pertencem ao tenant
        if (request.TagIds.Any() && !await _menuItemRepository.ValidateTagIdsAsync(request.TagIds, tenantId))
        {
            throw new BusinessRuleException("Um ou mais TagIds s�o inv�lidos para este tenant.", "TAG_VALIDATION_RULE");
        }
    }
}
