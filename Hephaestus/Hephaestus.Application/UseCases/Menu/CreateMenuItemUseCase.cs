using FluentValidation;
using Hephaestus.Application.DTOs.Request;
using Hephaestus.Application.Interfaces.Menu;
using Hephaestus.Domain.Entities;
using Hephaestus.Domain.Repositories;
using Hephaestus.Application.Exceptions;
using Hephaestus.Application.Base;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;
using Hephaestus.Application.Services;

namespace Hephaestus.Application.UseCases.Menu;

/// <summary>
/// Caso de uso para criação de itens do cardápio.
/// </summary>
public class CreateMenuItemUseCase : BaseUseCase, ICreateMenuItemUseCase
{
    private readonly IMenuItemRepository _menuItemRepository;
    private readonly ITagRepository _tagRepository;
    private readonly IValidator<CreateMenuItemRequest> _validator;

    /// <summary>
    /// Inicializa uma nova instância do <see cref="CreateMenuItemUseCase"/>.
    /// </summary>
    /// <param name="menuItemRepository">Repositório de itens do cardápio.</param>
    /// <param name="tagRepository">Repositório de tags.</param>
    /// <param name="validator">Validador para a requisição.</param>
    /// <param name="logger">Logger.</param>
    /// <param name="exceptionHandler">Serviço de tratamento de exceções.</param>
    public CreateMenuItemUseCase(
        IMenuItemRepository menuItemRepository,
        ITagRepository tagRepository,
        IValidator<CreateMenuItemRequest> validator,
        ILogger<CreateMenuItemUseCase> logger,
        IExceptionHandlerService exceptionHandler)
        : base(logger, exceptionHandler)
    {
        _menuItemRepository = menuItemRepository;
        _tagRepository = tagRepository;
        _validator = validator;
    }

    /// <summary>
    /// Executa a criação de um item do cardápio.
    /// </summary>
    /// <param name="request">Dados do item do cardápio a ser criado.</param>
    /// <param name="tenantId">ID do tenant.</param>
    /// <returns>ID do item criado.</returns>
    public async Task<string> ExecuteAsync(CreateMenuItemRequest request, string tenantId)
    {
        return await ExecuteWithExceptionHandlingAsync(async () =>
        {
            // Validação dos dados de entrada
            ValidateRequest(request);

            // Validação das regras de negócio
            await ValidateBusinessRulesAsync(request, tenantId);

            // Criação do item do cardápio
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
    /// Valida os dados da requisição.
    /// </summary>
    /// <param name="request">Requisição a ser validada.</param>
    private void ValidateRequest(CreateMenuItemRequest request)
    {
        if (request == null)
            throw new Hephaestus.Application.Exceptions.ValidationException("Dados do item do cardápio são obrigatórios.", new ValidationResult());
        if (string.IsNullOrEmpty(request.Name))
            throw new Hephaestus.Application.Exceptions.ValidationException("Nome do item é obrigatório.", new ValidationResult());
        if (string.IsNullOrEmpty(request.Description))
            throw new Hephaestus.Application.Exceptions.ValidationException("Descrição do item é obrigatória.", new ValidationResult());
    }

    /// <summary>
    /// Valida as regras de negócio.
    /// </summary>
    /// <param name="request">Requisição com os dados.</param>
    /// <param name="tenantId">ID do tenant.</param>
    private async Task ValidateBusinessRulesAsync(CreateMenuItemRequest request, string tenantId)
    {
        // Valida se as tags pertencem ao tenant
        if (request.TagIds.Any() && !await _menuItemRepository.ValidateTagIdsAsync(request.TagIds, tenantId))
        {
            throw new BusinessRuleException("Um ou mais TagIds são inválidos para este tenant.", "TAG_VALIDATION_RULE");
        }
    }
}