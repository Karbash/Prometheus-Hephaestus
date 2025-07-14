using Hephaestus.Application.DTOs.Response;
using Hephaestus.Application.Interfaces.Promotion;
using Hephaestus.Domain.Repositories;
using Hephaestus.Application.Exceptions;
using Hephaestus.Application.Base;
using Microsoft.Extensions.Logging;
using Hephaestus.Application.Services;
using FluentValidation.Results;

namespace Hephaestus.Application.UseCases.Promotion;

/// <summary>
/// Caso de uso para obter todas as promoções de um tenant.
/// </summary>
public class GetPromotionsUseCase : BaseUseCase, IGetPromotionsUseCase
{
    private readonly IPromotionRepository _promotionRepository;

    /// <summary>
    /// Inicializa uma nova instância do <see cref="GetPromotionsUseCase"/>.
    /// </summary>
    /// <param name="promotionRepository">Repositório de promoções.</param>
    /// <param name="logger">Logger.</param>
    /// <param name="exceptionHandler">Serviço de tratamento de exceções.</param>
    public GetPromotionsUseCase(
        IPromotionRepository promotionRepository,
        ILogger<GetPromotionsUseCase> logger,
        IExceptionHandlerService exceptionHandler)
        : base(logger, exceptionHandler)
    {
        _promotionRepository = promotionRepository;
    }

    /// <summary>
    /// Executa a busca de todas as promoções de um tenant.
    /// </summary>
    /// <param name="tenantId">ID do tenant.</param>
    /// <param name="isActive">Filtro opcional para promoções ativas.</param>
    /// <returns>Lista de promoções.</returns>
    public async Task<IEnumerable<PromotionResponse>> ExecuteAsync(string tenantId, bool? isActive)
    {
        return await ExecuteWithExceptionHandlingAsync(async () =>
        {
            // Validação dos parâmetros de entrada
            ValidateInputParameters(tenantId);

            // Busca das promoções
            var promotions = await GetPromotionsAsync(tenantId, isActive);

            // Conversão para DTOs de resposta
            return ConvertToResponseDtos(promotions);
        });
    }

    /// <summary>
    /// Valida os parâmetros de entrada.
    /// </summary>
    /// <param name="tenantId">ID do tenant.</param>
    private void ValidateInputParameters(string tenantId)
    {
        if (string.IsNullOrEmpty(tenantId))
            throw new Hephaestus.Application.Exceptions.ValidationException("ID do tenant é obrigatório.", new ValidationResult());
    }

    /// <summary>
    /// Busca as promoções.
    /// </summary>
    /// <param name="tenantId">ID do tenant.</param>
    /// <param name="isActive">Filtro opcional para promoções ativas.</param>
    /// <returns>Lista de promoções.</returns>
    private async Task<IEnumerable<Domain.Entities.Promotion>> GetPromotionsAsync(string tenantId, bool? isActive)
    {
        return await _promotionRepository.GetByTenantIdAsync(tenantId, isActive);
    }

    /// <summary>
    /// Converte as entidades para DTOs de resposta.
    /// </summary>
    /// <param name="promotions">Lista de promoções.</param>
    /// <returns>Lista de DTOs de resposta.</returns>
    private IEnumerable<PromotionResponse> ConvertToResponseDtos(IEnumerable<Domain.Entities.Promotion> promotions)
    {
        return promotions.Select(p => new PromotionResponse
        {
            Id = p.Id,
            TenantId = p.TenantId,
            Name = p.Name,
            Description = p.Description,
            DiscountType = p.DiscountType.ToString(),
            DiscountValue = p.DiscountValue,
            MenuItemId = p.MenuItemId,
            MinOrderValue = p.MinOrderValue,
            MaxUsagePerCustomer = p.MaxUsesPerCustomer,
            MaxTotalUses = p.MaxTotalUses,
            ApplicableToTags = p.ApplicableTags,
            StartDate = p.StartDate,
            EndDate = p.EndDate,
            IsActive = p.IsActive,
            ImageUrl = p.ImageUrl
        }).ToList();
    }
}