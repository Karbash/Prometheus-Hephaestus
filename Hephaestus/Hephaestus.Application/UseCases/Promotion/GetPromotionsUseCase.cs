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
/// Caso de uso para obter todas as promoções de um tenant.
/// </summary>
public class GetPromotionsUseCase : BaseUseCase, IGetPromotionsUseCase
{
    private readonly IPromotionRepository _promotionRepository;
    private readonly ILoggedUserService _loggedUserService;

    /// <summary>
    /// Inicializa uma nova instância do <see cref="GetPromotionsUseCase"/>.
    /// </summary>
    /// <param name="promotionRepository">Repositório de promoções.</param>
    /// <param name="loggedUserService">Serviço para obter informações do usuário logado.</param>
    /// <param name="logger">Logger.</param>
    /// <param name="exceptionHandler">Serviço de tratamento de exceções.</param>
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
    /// Executa a busca de todas as promoções de um tenant.
    /// </summary>
    /// <param name="user">Usuário autenticado.</param>
    /// <param name="isActive">Filtro opcional para promoções ativas.</param>
    /// <returns>Lista de promoções.</returns>
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
    private async Task<PagedResult<Domain.Entities.Promotion>> GetPromotionsAsync(string tenantId, bool? isActive, int pageNumber = 1, int pageSize = 20, string? sortBy = null, string? sortOrder = "asc")
    {
        return await _promotionRepository.GetByTenantIdAsync(tenantId, isActive, pageNumber, pageSize, sortBy, sortOrder);
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
            DiscountType = p.DiscountType,
            DiscountValue = p.DiscountValue,
            MenuItemId = p.MenuItemId,
            MinOrderValue = p.MinOrderValue,
            MaxUsesPerCustomer = p.MaxUsesPerCustomer,
            MaxTotalUses = p.MaxTotalUses,
            ApplicableToTags = p.ApplicableTags,
            StartDate = p.StartDate,
            EndDate = p.EndDate,
            IsActive = p.IsActive,
            ImageUrl = p.ImageUrl
        }).ToList();
    }
}