using FluentValidation;
using Hephaestus.Domain.DTOs.Request;
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
/// Caso de uso para atualiza��o de promo��es.
/// </summary>
public class UpdatePromotionUseCase : BaseUseCase, IUpdatePromotionUseCase
{
    private readonly IPromotionRepository _promotionRepository;
    private readonly IValidator<UpdatePromotionRequest> _validator;
    private readonly IMenuItemRepository _menuItemRepository;
    private readonly ILoggedUserService _loggedUserService;

    /// <summary>
    /// Inicializa uma nova inst�ncia do <see cref="UpdatePromotionUseCase"/>.
    /// </summary>
    /// <param name="promotionRepository">Reposit�rio de promo��es.</param>
    /// <param name="validator">Validador para a requisi��o.</param>
    /// <param name="menuItemRepository">Reposit�rio de itens do card�pio.</param>
    /// <param name="loggedUserService">Servi�o para obter informa��es do usu�rio logado.</param>
    /// <param name="logger">Logger.</param>
    /// <param name="exceptionHandler">Servi�o de tratamento de exce��es.</param>
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
    /// Executa a atualiza��o de uma promo��o.
    /// </summary>
    /// <param name="id">ID da promo��o.</param>
    /// <param name="request">Dados atualizados da promo��o.</param>
    /// <param name="user">Usu�rio autenticado.</param>
    public async Task ExecuteAsync(string id, UpdatePromotionRequest request, ClaimsPrincipal user)
    {
        await ExecuteWithExceptionHandlingAsync(async () =>
        {
            var companyId = _loggedUserService.GetCompanyId(user);
            
            // Validao dos dados de entrada
            await ValidateRequestAsync(request, id);

            // Busca e valida��o da promo��o
            var promotion = await GetAndValidatePromotionAsync(id, companyId);

            // Validao das regras de negcio
            await ValidateBusinessRulesAsync(request, companyId);

            // Atualizao da promoo
            await UpdatePromotionAsync(promotion, request);
        });
    }

    /// <summary>
    /// Valida os dados da requisi��o.
    /// </summary>
    /// <param name="request">Requisi��o a ser validada.</param>
    /// <param name="id">ID da promo��o.</param>
    private async Task ValidateRequestAsync(UpdatePromotionRequest request, string id)
    {
        var validationResult = await _validator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            throw new Hephaestus.Application.Exceptions.ValidationException("Dados da promo��o inv�lidos", validationResult);
        }
    }

    /// <summary>
    /// Busca e valida a promo��o.
    /// </summary>
    /// <param name="id">ID da promo��o.</param>
    /// <param name="companyId">ID do tenant.</param>
    /// <returns>Promoo encontrada.</returns>
    private async Task<Domain.Entities.Promotion> GetAndValidatePromotionAsync(string id, string companyId)
    {
        var promotion = await _promotionRepository.GetByIdAsync(id, companyId);
        if (promotion == null)
            throw new NotFoundException("Promotion", id);
        return promotion;
    }

    /// <summary>
    /// Valida as regras de negcio.
    /// </summary>
    /// <param name="request">Requisi��o com os dados.</param>
    /// <param name="companyId">ID do tenant.</param>
    private async Task ValidateBusinessRulesAsync(UpdatePromotionRequest request, string companyId)
    {
        if (request.DiscountType == DiscountType.FreeItem && !string.IsNullOrEmpty(request.MenuItemId))
        {
            var menuItem = await _menuItemRepository.GetByIdAsync(request.MenuItemId, companyId);
            EnsureEntityExists(menuItem, "MenuItem", request.MenuItemId);
        }
    }

    /// <summary>
    /// Atualiza a promoo com os novos dados.
    /// </summary>
    /// <param name="promotion">Promo��o a ser atualizada.</param>
    /// <param name="request">Dados atualizados.</param>
    private async Task UpdatePromotionAsync(Domain.Entities.Promotion promotion, UpdatePromotionRequest request)
    {
        promotion.Name = request.Name;
        promotion.Description = request.Description;
        promotion.DiscountType = request.DiscountType;
        promotion.DiscountValue = request.DiscountValue;
        promotion.MenuItemId = request.MenuItemId;
        promotion.MinOrderValue = request.MinOrderValue;
        promotion.MaxUsesPerCustomer = request.MaxUsesPerCustomer;
        promotion.MaxTotalUses = request.MaxTotalUses;
        // ApplicableTags foi removido da entidade
        promotion.StartDate = request.StartDate;
        promotion.EndDate = request.EndDate;
        promotion.IsActive = request.IsActive;
        promotion.ImageUrl = request.ImageUrl;

        await _promotionRepository.UpdateAsync(promotion);
    }
}
