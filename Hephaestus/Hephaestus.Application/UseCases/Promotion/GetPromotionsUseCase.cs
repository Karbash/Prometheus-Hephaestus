using Hephaestus.Domain.DTOs.Response;
using Hephaestus.Application.Interfaces.Promotion;
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
/// Caso de uso para obter todas as promo��es de um tenant.
/// </summary>
public class GetPromotionsUseCase : BaseUseCase, IGetPromotionsUseCase
{
    private readonly IPromotionRepository _promotionRepository;
    private readonly ILoggedUserService _loggedUserService;

    /// <summary>
    /// Inicializa uma nova inst�ncia do <see cref="GetPromotionsUseCase"/>.
    /// </summary>
    /// <param name="promotionRepository">Reposit�rio de promo��es.</param>
    /// <param name="loggedUserService">Servi�o para obter informa��es do usu�rio logado.</param>
    /// <param name="logger">Logger.</param>
    /// <param name="exceptionHandler">Servi�o de tratamento de exce��es.</param>
    public GetPromotionsUseCase(
        IPromotionRepository promotionRepository,
        ILoggedUserService loggedUserService,
        ILogger<GetPromotionsUseCase> logger,
        IExceptionHandlerService exceptionHandler)
        : base(logger, exceptionHandler)
    {
        _promotionRepository = promotionRepository;
        _loggedUserService = loggedUserService;
    }

    /// <summary>
    /// Executa a busca de todas as promo��es de um tenant.
    /// </summary>
    /// <param name="user">Usu�rio autenticado.</param>
    /// <param name="isActive">Filtro opcional para promo��es ativas.</param>
    /// <returns>Lista de promo��es.</returns>
    public async Task<PagedResult<PromotionResponse>> ExecuteAsync(ClaimsPrincipal user, bool? isActive, int pageNumber = 1, int pageSize = 20, string? sortBy = null, string? sortOrder = "asc")
    {
        return await ExecuteWithExceptionHandlingAsync(async () =>
        {
            var tenantId = _loggedUserService.GetTenantId(user);
            ValidateInputParameters(tenantId);
            var pagedPromotions = await _promotionRepository.GetByTenantIdAsync(tenantId, isActive, pageNumber, pageSize, sortBy, sortOrder);
            return new PagedResult<PromotionResponse>
            {
                Items = ConvertToResponseDtos(pagedPromotions.Items).ToList(),
                TotalCount = pagedPromotions.TotalCount,
                PageNumber = pagedPromotions.PageNumber,
                PageSize = pagedPromotions.PageSize
            };
        });
    }

    /// <summary>
    /// Valida os par�metros de entrada.
    /// </summary>
    /// <param name="tenantId">ID do tenant.</param>
    private void ValidateInputParameters(string tenantId)
    {
        if (string.IsNullOrEmpty(tenantId))
            throw new Hephaestus.Application.Exceptions.ValidationException("ID do tenant � obrigat�rio.", new ValidationResult());
    }

    /// <summary>
    /// Busca as promo��es.
    /// </summary>
    /// <param name="tenantId">ID do tenant.</param>
    /// <param name="isActive">Filtro opcional para promo��es ativas.</param>
    /// <returns>Lista de promo��es.</returns>
    private async Task<PagedResult<Domain.Entities.Promotion>> GetPromotionsAsync(string tenantId, bool? isActive, int pageNumber = 1, int pageSize = 20, string? sortBy = null, string? sortOrder = "asc")
    {
        return await _promotionRepository.GetByTenantIdAsync(tenantId, isActive, pageNumber, pageSize, sortBy, sortOrder);
    }

    /// <summary>
    /// Converte as entidades para DTOs de resposta.
    /// </summary>
    /// <param name="promotions">Lista de promo��es.</param>
    /// <returns>Lista de DTOs de resposta.</returns>
    private IEnumerable<PromotionResponse> ConvertToResponseDtos(IEnumerable<Domain.Entities.Promotion> promotions)
    {
        return promotions.Select(p => new PromotionResponse
        {
            Id = p.Id,
            TenantId = p.TenantId,
            Name = p.Name,
            Description = p.Description,
            DiscountType = p.DiscountType,
            DiscountValue = p.DiscountValue,
            MenuItemId = p.MenuItemId,
            MinOrderValue = p.MinOrderValue,
            MaxUsesPerCustomer = p.MaxUsesPerCustomer,
            MaxTotalUses = p.MaxTotalUses,
            // ApplicableToTags foi removido da entidade
            StartDate = p.StartDate,
            EndDate = p.EndDate,
            IsActive = p.IsActive,
            ImageUrl = p.ImageUrl
        }).ToList();
    }
}
