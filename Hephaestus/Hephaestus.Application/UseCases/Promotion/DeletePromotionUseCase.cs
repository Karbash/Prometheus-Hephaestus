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
            var companyId = _loggedUserService.GetCompanyId(user);
            
            // Validao dos par�metros de entrada
            ValidateInputParameters(id, companyId);

            // Verifica se a promoo existe antes de tentar remov-la
            await ValidatePromotionExistsAsync(id, companyId);

            // Remove a promoo
            await _promotionRepository.DeleteAsync(id, companyId);
        });
    }

    /// <summary>
    /// Valida os parmetros de entrada.
    /// </summary>
    /// <param name="id">ID da promoo.</param>
    /// <param name="companyId">ID da empresa.</param>
    private void ValidateInputParameters(string id, string companyId)
    {
        if (string.IsNullOrEmpty(id))
            throw new Hephaestus.Application.Exceptions.ValidationException("ID da promoo obrigatrio.", new ValidationResult());

        if (string.IsNullOrEmpty(companyId))
            throw new Hephaestus.Application.Exceptions.ValidationException("ID da empresa obrigatrio.", new ValidationResult());
    }

    /// <summary>
    /// Verifica se a promoo existe.
    /// </summary>
    /// <param name="id">ID da promoo.</param>
    /// <param name="companyId">ID da empresa.</param>
    private async Task ValidatePromotionExistsAsync(string id, string companyId)
    {
        var promotion = await _promotionRepository.GetByIdAsync(id, companyId);
        EnsureEntityExists(promotion, "Promotion", id);
    }
}
