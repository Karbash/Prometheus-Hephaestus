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
/// Caso de uso para remo��o de promo��es.
/// </summary>
public class DeletePromotionUseCase : BaseUseCase, IDeletePromotionUseCase
{
    private readonly IPromotionRepository _promotionRepository;
    private readonly ILoggedUserService _loggedUserService;

    /// <summary>
    /// Inicializa uma nova inst�ncia do <see cref="DeletePromotionUseCase"/>.
    /// </summary>
    /// <param name="promotionRepository">Reposit�rio de promo��es.</param>
    /// <param name="loggedUserService">Servi�o para obter informa��es do usu�rio logado.</param>
    /// <param name="logger">Logger.</param>
    /// <param name="exceptionHandler">Servi�o de tratamento de exce��es.</param>
    public DeletePromotionUseCase(
        IPromotionRepository promotionRepository,
        ILoggedUserService loggedUserService,
        ILogger<DeletePromotionUseCase> logger,
        IExceptionHandlerService exceptionHandler)
        : base(logger, exceptionHandler)
    {
        _promotionRepository = promotionRepository;
        _loggedUserService = loggedUserService;
    }

    /// <summary>
    /// Executa a remo��o de uma promo��o.
    /// </summary>
    /// <param name="id">ID da promo��o.</param>
    /// <param name="user">Usu�rio autenticado.</param>
    public async Task ExecuteAsync(string id, ClaimsPrincipal user)
    {
        await ExecuteWithExceptionHandlingAsync(async () =>
        {
            var tenantId = _loggedUserService.GetTenantId(user);
            
            // Valida��o dos par�metros de entrada
            ValidateInputParameters(id, tenantId);

            // Verifica se a promo��o existe antes de tentar remov�-la
            await ValidatePromotionExistsAsync(id, tenantId);

            // Remove a promo��o
            await _promotionRepository.DeleteAsync(id, tenantId);
        });
    }

    /// <summary>
    /// Valida os par�metros de entrada.
    /// </summary>
    /// <param name="id">ID da promo��o.</param>
    /// <param name="tenantId">ID do tenant.</param>
    private void ValidateInputParameters(string id, string tenantId)
    {
        if (string.IsNullOrEmpty(id))
            throw new Hephaestus.Application.Exceptions.ValidationException("ID da promo��o � obrigat�rio.", new ValidationResult());

        if (string.IsNullOrEmpty(tenantId))
            throw new Hephaestus.Application.Exceptions.ValidationException("ID do tenant � obrigat�rio.", new ValidationResult());
    }

    /// <summary>
    /// Verifica se a promo��o existe.
    /// </summary>
    /// <param name="id">ID da promo��o.</param>
    /// <param name="tenantId">ID do tenant.</param>
    private async Task ValidatePromotionExistsAsync(string id, string tenantId)
    {
        var promotion = await _promotionRepository.GetByIdAsync(id, tenantId);
        EnsureEntityExists(promotion, "Promotion", id);
    }
}
