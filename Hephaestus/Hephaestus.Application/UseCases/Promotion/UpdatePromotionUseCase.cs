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
using FluentValidation.Results;
using System.Security.Claims;

namespace Hephaestus.Application.UseCases.Promotion;

/// <summary>
/// Caso de uso para atualização de promoções.
/// </summary>
public class UpdatePromotionUseCase : BaseUseCase, IUpdatePromotionUseCase
{
    private readonly IPromotionRepository _promotionRepository;
    private readonly IValidator<UpdatePromotionRequest> _validator;
    private readonly IMenuItemRepository _menuItemRepository;
    private readonly ILoggedUserService _loggedUserService;

    /// <summary>
    /// Inicializa uma nova instância do <see cref="UpdatePromotionUseCase"/>.
    /// </summary>
    /// <param name="promotionRepository">Repositório de promoções.</param>
    /// <param name="validator">Validador para a requisição.</param>
    /// <param name="menuItemRepository">Repositório de itens do cardápio.</param>
    /// <param name="loggedUserService">Serviço para obter informações do usuário logado.</param>
    /// <param name="logger">Logger.</param>
    /// <param name="exceptionHandler">Serviço de tratamento de exceções.</param>
    public UpdatePromotionUseCase(
        IPromotionRepository promotionRepository,
        IValidator<UpdatePromotionRequest> validator,
        IMenuItemRepository menuItemRepository,
        ILoggedUserService loggedUserService,
        ILogger<UpdatePromotionUseCase> logger,
        IExceptionHandlerService exceptionHandler)
        : base(logger, exceptionHandler)
    {
        _promotionRepository = promotionRepository;
        _validator = validator;
        _menuItemRepository = menuItemRepository;
        _loggedUserService = loggedUserService;
    }

    /// <summary>
    /// Executa a atualização de uma promoção.
    /// </summary>
    /// <param name="id">ID da promoção.</param>
    /// <param name="request">Dados atualizados da promoção.</param>
    /// <param name="user">Usuário autenticado.</param>
    public async Task ExecuteAsync(string id, UpdatePromotionRequest request, ClaimsPrincipal user)
    {
        await ExecuteWithExceptionHandlingAsync(async () =>
        {
            var tenantId = _loggedUserService.GetTenantId(user);
            
            // Validação dos dados de entrada
            await ValidateRequestAsync(request, id);

            // Busca e validação da promoção
            var promotion = await GetAndValidatePromotionAsync(id, tenantId);

            // Validação das regras de negócio
            await ValidateBusinessRulesAsync(request, tenantId);

            // Atualização da promoção
            await UpdatePromotionAsync(promotion, request);
        });
    }

    /// <summary>
    /// Valida os dados da requisição.
    /// </summary>
    /// <param name="request">Requisição a ser validada.</param>
    /// <param name="id">ID da promoção.</param>
    private async Task ValidateRequestAsync(UpdatePromotionRequest request, string id)
    {
        var validationResult = await _validator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            throw new Hephaestus.Application.Exceptions.ValidationException("Dados da promoção inválidos", validationResult);
        }
    }

    /// <summary>
    /// Busca e valida a promoção.
    /// </summary>
    /// <param name="id">ID da promoção.</param>
    /// <param name="tenantId">ID do tenant.</param>
    /// <returns>Promoção encontrada.</returns>
    private async Task<Domain.Entities.Promotion> GetAndValidatePromotionAsync(string id, string tenantId)
    {
        var promotion = await _promotionRepository.GetByIdAsync(id, tenantId);
        EnsureEntityExists(promotion, "Promotion", id);
        return promotion!; // Garantido que não é null após EnsureEntityExists
    }

    /// <summary>
    /// Valida as regras de negócio.
    /// </summary>
    /// <param name="request">Requisição com os dados.</param>
    /// <param name="tenantId">ID do tenant.</param>
    private async Task ValidateBusinessRulesAsync(UpdatePromotionRequest request, string tenantId)
    {
        if (request.DiscountType == "FreeItem" && !string.IsNullOrEmpty(request.MenuItemId))
        {
            var menuItem = await _menuItemRepository.GetByIdAsync(request.MenuItemId, tenantId);
            EnsureEntityExists(menuItem, "MenuItem", request.MenuItemId);
        }

        if (!Enum.TryParse<DiscountType>(request.DiscountType, true, out _))
        {
            throw new Hephaestus.Application.Exceptions.ValidationException("Tipo de desconto inválido.", new ValidationResult());
        }
    }

    private DiscountType ParseDiscountType(string discountTypeStr)
    {
        if (!Enum.TryParse<DiscountType>(discountTypeStr, true, out var discountType))
        {
            throw new BusinessRuleException($"Tipo de desconto inválido: {discountTypeStr}. Os valores válidos são: {string.Join(", ", Enum.GetNames(typeof(DiscountType)))}.", "DISCOUNT_TYPE_VALIDATION");
        }
        return discountType;
    }

    /// <summary>
    /// Atualiza a promoção com os novos dados.
    /// </summary>
    /// <param name="promotion">Promoção a ser atualizada.</param>
    /// <param name="request">Dados atualizados.</param>
    private async Task UpdatePromotionAsync(Domain.Entities.Promotion promotion, UpdatePromotionRequest request)
    {
        promotion.Name = request.Name;
        promotion.Description = request.Description;
        promotion.DiscountType = ParseDiscountType(request.DiscountType);
        promotion.DiscountValue = request.DiscountValue;
        promotion.MenuItemId = request.MenuItemId;
        promotion.MinOrderValue = request.MinOrderValue;
        promotion.MaxUsesPerCustomer = request.MaxUsagePerCustomer;
        promotion.MaxTotalUses = request.MaxTotalUses;
        promotion.ApplicableTags = request.ApplicableToTags ?? new List<string>();
        promotion.StartDate = request.StartDate;
        promotion.EndDate = request.EndDate;
        promotion.IsActive = request.IsActive;
        promotion.ImageUrl = request.ImageUrl;

        await _promotionRepository.UpdateAsync(promotion);
    }
}