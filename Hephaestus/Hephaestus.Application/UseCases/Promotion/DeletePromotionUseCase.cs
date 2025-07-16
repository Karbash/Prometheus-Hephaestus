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
/// Caso de uso para remoção de promoções.
/// </summary>
public class DeletePromotionUseCase : BaseUseCase, IDeletePromotionUseCase
{
    private readonly IPromotionRepository _promotionRepository;
    private readonly ILoggedUserService _loggedUserService;

    /// <summary>
    /// Inicializa uma nova instância do <see cref="DeletePromotionUseCase"/>.
    /// </summary>
    /// <param name="promotionRepository">Repositório de promoções.</param>
    /// <param name="loggedUserService">Serviço para obter informações do usuário logado.</param>
    /// <param name="logger">Logger.</param>
    /// <param name="exceptionHandler">Serviço de tratamento de exceções.</param>
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
    /// Executa a remoção de uma promoção.
    /// </summary>
    /// <param name="id">ID da promoção.</param>
    /// <param name="user">Usuário autenticado.</param>
    public async Task ExecuteAsync(string id, ClaimsPrincipal user)
    {
        await ExecuteWithExceptionHandlingAsync(async () =>
        {
            var tenantId = _loggedUserService.GetTenantId(user);
            
            // Validação dos parâmetros de entrada
            ValidateInputParameters(id, tenantId);

            // Verifica se a promoção existe antes de tentar removê-la
            await ValidatePromotionExistsAsync(id, tenantId);

            // Remove a promoção
            await _promotionRepository.DeleteAsync(id, tenantId);
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
    /// Verifica se a promoção existe.
    /// </summary>
    /// <param name="id">ID da promoção.</param>
    /// <param name="tenantId">ID do tenant.</param>
    private async Task ValidatePromotionExistsAsync(string id, string tenantId)
    {
        var promotion = await _promotionRepository.GetByIdAsync(id, tenantId);
        EnsureEntityExists(promotion, "Promotion", id);
    }
}
