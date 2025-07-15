using Hephaestus.Domain.DTOs.Response;
using Hephaestus.Application.Interfaces.Promotion;
using Hephaestus.Domain.Repositories;
using Hephaestus.Application.Exceptions;
using Hephaestus.Application.Base;
using Hephaestus.Domain.Services;
using Microsoft.Extensions.Logging;
using Hephaestus.Application.Services;
using FluentValidation.Results;
using Hephaestus.Domain.Enum;
using System.Security.Claims;

namespace Hephaestus.Application.UseCases.Promotion;

/// <summary>
/// Caso de uso para obter uma promoção específica por ID.
/// </summary>
public class GetPromotionByIdUseCase : BaseUseCase, IGetPromotionByIdUseCase
{
    private readonly IPromotionRepository _promotionRepository;
    private readonly ILoggedUserService _loggedUserService;

    /// <summary>
    /// Inicializa uma nova instância do <see cref="GetPromotionByIdUseCase"/>.
    /// </summary>
    /// <param name="promotionRepository">Repositório de promoções.</param>
    /// <param name="loggedUserService">Serviço para obter informações do usuário logado.</param>
    /// <param name="logger">Logger.</param>
    /// <param name="exceptionHandler">Serviço de tratamento de exceções.</param>
    public GetPromotionByIdUseCase(
        IPromotionRepository promotionRepository,
        ILoggedUserService loggedUserService,
        ILogger<GetPromotionByIdUseCase> logger,
        IExceptionHandlerService exceptionHandler)
        : base(logger, exceptionHandler)
    {
        _promotionRepository = promotionRepository;
        _loggedUserService = loggedUserService;
    }

    /// <summary>
    /// Executa a busca de uma promoção específica.
    /// </summary>
    /// <param name="id">ID da promoção.</param>
    /// <param name="user">Usuário autenticado.</param>
    /// <returns>Promoção encontrada.</returns>
    public async Task<PromotionResponse> ExecuteAsync(string id, ClaimsPrincipal user)
    {
        return await ExecuteWithExceptionHandlingAsync(async () =>
        {
            var tenantId = _loggedUserService.GetTenantId(user);
            
            // Validação dos parâmetros de entrada
            ValidateInputParameters(id, tenantId);

            // Busca e validação da promoção
            var promotion = await GetAndValidatePromotionAsync(id, tenantId);

            // Conversão para DTO de resposta
            return ConvertToResponseDto(promotion);
        });
    }

    /// <summary>
    /// Valida os parâmetros de entrada.
    /// </summary>
    /// <param name="id">ID da promoção.</param>
    /// <param name="tenantId">ID do tenant.</param>
    private void ValidateInputParameters(string id, string tenantId)
    {
        if (string.IsNullOrEmpty(id))
            throw new Hephaestus.Application.Exceptions.ValidationException("ID da promoção é obrigatório.", new ValidationResult());

        if (string.IsNullOrEmpty(tenantId))
            throw new Hephaestus.Application.Exceptions.ValidationException("ID do tenant é obrigatório.", new ValidationResult());
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
    /// Converte a entidade para DTO de resposta.
    /// </summary>
    /// <param name="promotion">Promoção encontrada.</param>
    /// <returns>DTO de resposta.</returns>
    private PromotionResponse ConvertToResponseDto(Domain.Entities.Promotion promotion)
    {
        return new PromotionResponse
        {
            Id = promotion.Id,
            TenantId = promotion.TenantId,
            Name = promotion.Name,
            Description = promotion.Description,
            DiscountType = promotion.DiscountType,
            DiscountValue = promotion.DiscountValue,
            MenuItemId = promotion.MenuItemId,
            MinOrderValue = promotion.MinOrderValue,
            MaxUsesPerCustomer = promotion.MaxUsesPerCustomer,
            MaxTotalUses = promotion.MaxTotalUses,
            ApplicableToTags = promotion.ApplicableTags,
            StartDate = promotion.StartDate,
            EndDate = promotion.EndDate,
            IsActive = promotion.IsActive,
            ImageUrl = promotion.ImageUrl
        };
    }
}