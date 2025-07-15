using FluentValidation;
using Hephaestus.Application.DTOs.Request;
using Hephaestus.Application.Interfaces.Promotion;
using Hephaestus.Domain.Enum;
using Hephaestus.Domain.Repositories;
using Hephaestus.Application.Exceptions;
using Hephaestus.Application.Base;
using Hephaestus.Domain.Services;
using Microsoft.Extensions.Logging;
using Hephaestus.Application.Services;
using System.Security.Claims;

namespace Hephaestus.Application.UseCases.Promotion;

/// <summary>
/// Caso de uso para criação de promoções.
/// </summary>
public class CreatePromotionUseCase : BaseUseCase, ICreatePromotionUseCase
{
    private readonly IPromotionRepository _promotionRepository;
    private readonly IValidator<CreatePromotionRequest> _validator;
    private readonly IMenuItemRepository _menuItemRepository;
    private readonly ILoggedUserService _loggedUserService;

    /// <summary>
    /// Inicializa uma nova instância do <see cref="CreatePromotionUseCase"/>.
    /// </summary>
    /// <param name="promotionRepository">Repositório de promoções.</param>
    /// <param name="validator">Validador para a requisição.</param>
    /// <param name="menuItemRepository">Repositório de itens do cardápio.</param>
    /// <param name="loggedUserService">Serviço para obter informações do usuário logado.</param>
    /// <param name="logger">Logger.</param>
    /// <param name="exceptionHandler">Serviço de tratamento de exceções.</param>
    public CreatePromotionUseCase(
        IPromotionRepository promotionRepository,
        IValidator<CreatePromotionRequest> validator,
        IMenuItemRepository menuItemRepository,
        ILoggedUserService loggedUserService,
        ILogger<CreatePromotionUseCase> logger,
        IExceptionHandlerService exceptionHandler)
        : base(logger, exceptionHandler)
    {
        _promotionRepository = promotionRepository;
        _validator = validator;
        _menuItemRepository = menuItemRepository;
        _loggedUserService = loggedUserService;
    }

    /// <summary>
    /// Executa a criação de uma promoção.
    /// </summary>
    /// <param name="request">Dados da promoção a ser criada.</param>
    /// <param name="user">Usuário autenticado.</param>
    /// <returns>ID da promoção criada.</returns>
    public async Task<string> ExecuteAsync(CreatePromotionRequest request, ClaimsPrincipal user)
    {
        return await ExecuteWithExceptionHandlingAsync(async () =>
        {
            var tenantId = _loggedUserService.GetTenantId(user);
            
            // Validação dos dados de entrada
            await ValidateRequestAsync(request);

            // Validação das regras de negócio
            await ValidateBusinessRulesAsync(request, tenantId);

            // Criação da promoção
            var promotion = await CreatePromotionEntityAsync(request, tenantId);

            // Persistência no repositório
            await _promotionRepository.AddAsync(promotion);

            return promotion.Id;
        });
    }

    /// <summary>
    /// Valida os dados da requisição.
    /// </summary>
    /// <param name="request">Requisição a ser validada.</param>
    private async Task ValidateRequestAsync(CreatePromotionRequest request)
    {
        var validationResult = await _validator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            throw new Hephaestus.Application.Exceptions.ValidationException("Dados da promoção inválidos", validationResult);
        }
    }

    /// <summary>
    /// Valida as regras de negócio.
    /// </summary>
    /// <param name="request">Requisição com os dados.</param>
    /// <param name="tenantId">ID do tenant.</param>
    private async Task ValidateBusinessRulesAsync(CreatePromotionRequest request, string tenantId)
    {
        // Valida se o item do cardápio existe para promoções FreeItem
        if (request.DiscountType == DiscountType.FreeItem && !string.IsNullOrEmpty(request.MenuItemId))
        {
            var menuItem = await _menuItemRepository.GetByIdAsync(request.MenuItemId, tenantId);
            if (menuItem == null)
            {
                throw new NotFoundException("MenuItem", request.MenuItemId);
            }
        }
    }

    /// <summary>
    /// Cria a entidade de promoção.
    /// </summary>
    /// <param name="request">Dados da promoção.</param>
    /// <param name="tenantId">ID do tenant.</param>
    /// <returns>Entidade de promoção criada.</returns>
    private Task<Domain.Entities.Promotion> CreatePromotionEntityAsync(CreatePromotionRequest request, string tenantId)
    {
        return Task.FromResult(new Domain.Entities.Promotion
        {
            Id = Guid.NewGuid().ToString(),
            TenantId = tenantId,
            Name = request.Name,
            Description = request.Description,
            DiscountType = request.DiscountType,
            DiscountValue = request.DiscountValue,
            MenuItemId = request.MenuItemId,
            MinOrderValue = request.MinOrderValue,
            MaxUsesPerCustomer = request.MaxUsesPerCustomer,
            MaxTotalUses = request.MaxTotalUses,
            ApplicableTags = request.ApplicableToTags ?? new List<string>(),
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            IsActive = request.IsActive,
            ImageUrl = request.ImageUrl
        });
    }
}