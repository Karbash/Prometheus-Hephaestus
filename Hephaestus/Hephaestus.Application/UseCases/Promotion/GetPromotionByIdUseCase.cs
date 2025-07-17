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
/// Caso de uso para obter uma promo��o espec�fica por ID.
/// </summary>
public class GetPromotionByIdUseCase : BaseUseCase, IGetPromotionByIdUseCase
{
    private readonly IPromotionRepository _promotionRepository;
    private readonly ILoggedUserService _loggedUserService;

    /// <summary>
    /// Inicializa uma nova inst�ncia do <see cref="GetPromotionByIdUseCase"/>.
    /// </summary>
    /// <param name="promotionRepository">Reposit�rio de promo��es.</param>
    /// <param name="loggedUserService">Servi�o para obter informa��es do usu�rio logado.</param>
    /// <param name="logger">Logger.</param>
    /// <param name="exceptionHandler">Servi�o de tratamento de exce��es.</param>
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
    /// Executa a busca de uma promo��o espec�fica.
    /// </summary>
    /// <param name="id">ID da promo��o.</param>
    /// <param name="user">Usu�rio autenticado.</param>
    /// <returns>Promo��o encontrada.</returns>
    public async Task<PromotionResponse> ExecuteAsync(string id, ClaimsPrincipal user)
    {
        return await ExecuteWithExceptionHandlingAsync(async () =>
        {
            var companyId = _loggedUserService.GetCompanyId(user);
            
            // Validao dos par�metros de entrada
            ValidateInputParameters(id, companyId);

            // Busca e validao da promoo
            var promotion = await GetAndValidatePromotionAsync(id, companyId);

            // Converso para DTO de resposta
            return ConvertToResponseDto(promotion);
        });
    }

    /// <summary>
    /// Valida os parmetros de entrada.
    /// </summary>
    /// <param name="id">ID da promo��o.</param>
    /// <param name="companyId">ID da empresa.</param>
    private void ValidateInputParameters(string id, string companyId)
    {
        if (string.IsNullOrEmpty(id))
            throw new Hephaestus.Application.Exceptions.ValidationException("ID da promoo obrigatrio.", new ValidationResult());

        if (string.IsNullOrEmpty(companyId))
            throw new Hephaestus.Application.Exceptions.ValidationException("ID da empresa obrigatrio.", new ValidationResult());
    }

    /// <summary>
    /// Busca e valida a promoo.
    /// </summary>
    /// <param name="id">ID da promoo.</param>
    /// <param name="companyId">ID da empresa.</param>
    /// <returns>Promoo encontrada.</returns>
    private async Task<Domain.Entities.Promotion> GetAndValidatePromotionAsync(string id, string companyId)
    {
        var promotion = await _promotionRepository.GetByIdAsync(id, companyId);
        EnsureEntityExists(promotion, "Promotion", id);
        return promotion!; // Garantido que no null aps EnsureEntityExists
    }

    /// <summary>
    /// Converte a entidade para DTO de resposta.
    /// </summary>
    /// <param name="promotion">Promoo encontrada.</param>
    /// <returns>DTO de resposta.</returns>
    private PromotionResponse ConvertToResponseDto(Domain.Entities.Promotion promotion)
    {
        return new PromotionResponse
        {
            Id = promotion.Id,
            CompanyId = promotion.CompanyId,
            Name = promotion.Name,
            Description = promotion.Description,
            DiscountType = promotion.DiscountType,
            DiscountValue = promotion.DiscountValue,
            MenuItemId = promotion.MenuItemId,
            MinOrderValue = promotion.MinOrderValue,
            MaxUsesPerCustomer = promotion.MaxUsesPerCustomer,
            MaxTotalUses = promotion.MaxTotalUses,
            // ApplicableToTags foi removido da entidade
            StartDate = promotion.StartDate,
            EndDate = promotion.EndDate,
            IsActive = promotion.IsActive,
            ImageUrl = promotion.ImageUrl
        };
    }
}
