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
using System.Security.Claims;

namespace Hephaestus.Application.UseCases.Promotion;

/// <summary>
/// Caso de uso para cria��o de promo��es.
/// </summary>
public class CreatePromotionUseCase : BaseUseCase, ICreatePromotionUseCase
{
    private readonly IPromotionRepository _promotionRepository;
    private readonly IValidator<CreatePromotionRequest> _validator;
    private readonly IMenuItemRepository _menuItemRepository;
    private readonly ILoggedUserService _loggedUserService;

    /// <summary>
    /// Inicializa uma nova inst�ncia do <see cref="CreatePromotionUseCase"/>.
    /// </summary>
    /// <param name="promotionRepository">Reposit�rio de promo��es.</param>
    /// <param name="validator">Validador para a requisi��o.</param>
    /// <param name="menuItemRepository">Reposit�rio de itens do card�pio.</param>
    /// <param name="loggedUserService">Servi�o para obter informa��es do usu�rio logado.</param>
    /// <param name="logger">Logger.</param>
    /// <param name="exceptionHandler">Servi�o de tratamento de exce��es.</param>
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
    /// Executa a cria��o de uma promo��o.
    /// </summary>
    /// <param name="request">Dados da promo��o a ser criada.</param>
    /// <param name="user">Usu�rio autenticado.</param>
    /// <returns>ID da promo��o criada.</returns>
    public async Task<string> ExecuteAsync(CreatePromotionRequest request, ClaimsPrincipal user)
    {
        return await ExecuteWithExceptionHandlingAsync(async () =>
        {
            var companyId = _loggedUserService.GetCompanyId(user);
            
            // Validao dos dados de entrada
            await ValidateRequestAsync(request);

            // Valida��o das regras de neg�cio
            await ValidateBusinessRulesAsync(request, companyId);

            // Criao da promoo
            var promotion = await CreatePromotionEntityAsync(request, companyId);

            // Persistncia no repositrio
            await _promotionRepository.AddAsync(promotion);

            return promotion.Id;
        });
    }

    /// <summary>
    /// Valida os dados da requisi��o.
    /// </summary>
    /// <param name="request">Requisi��o a ser validada.</param>
    private async Task ValidateRequestAsync(CreatePromotionRequest request)
    {
        var validationResult = await _validator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            throw new Hephaestus.Application.Exceptions.ValidationException("Dados da promo��o inv�lidos", validationResult);
        }
    }

    /// <summary>
    /// Valida as regras de neg�cio.
    /// </summary>
    /// <param name="request">Requisi��o com os dados.</param>
    /// <param name="companyId">ID do tenant.</param>
    private async Task ValidateBusinessRulesAsync(CreatePromotionRequest request, string companyId)
    {
        // Valida se o item do cardpio existe para promoes FreeItem
        if (request.DiscountType == DiscountType.FreeItem && !string.IsNullOrEmpty(request.MenuItemId))
        {
            var menuItem = await _menuItemRepository.GetByIdAsync(request.MenuItemId, companyId);
            if (menuItem == null)
            {
                throw new NotFoundException("MenuItem", request.MenuItemId);
            }
        }
    }

    /// <summary>
    /// Cria a entidade de promo��o.
    /// </summary>
    /// <param name="request">Dados da promo��o.</param>
    /// <param name="companyId">ID do tenant.</param>
    /// <returns>Entidade de promoo criada.</returns>
    private Task<Domain.Entities.Promotion> CreatePromotionEntityAsync(CreatePromotionRequest request, string companyId)
    {
        return Task.FromResult(new Domain.Entities.Promotion
        {
            Id = Guid.NewGuid().ToString(),
            CompanyId = companyId,
            Name = request.Name,
            Description = request.Description,
            DiscountType = request.DiscountType,
            DiscountValue = request.DiscountValue,
            MenuItemId = request.MenuItemId,
            MinOrderValue = request.MinOrderValue,
            MaxUsesPerCustomer = request.MaxUsesPerCustomer,
            MaxTotalUses = request.MaxTotalUses,
            // ApplicableTags foi removido da entidade
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            IsActive = request.IsActive,
            ImageUrl = request.ImageUrl
        });
    }
}
