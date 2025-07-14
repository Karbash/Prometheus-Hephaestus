using FluentValidation;
using FluentValidation.Results;
using Hephaestus.Application.DTOs.Request;
using Hephaestus.Application.Interfaces.Promotion;
using Hephaestus.Domain.Repositories;
using Hephaestus.Application.Exceptions;
using Hephaestus.Application.Base;
using Hephaestus.Domain.Services;
using Microsoft.Extensions.Logging;
using Hephaestus.Application.Services;
using System.Security.Claims;
using ValidationException = Hephaestus.Application.Exceptions.ValidationException;

namespace Hephaestus.Application.UseCases.Promotion;

/// <summary>
/// Caso de uso para notificação de promoções.
/// </summary>
public class NotifyPromotionUseCase : BaseUseCase, INotifyPromotionUseCase
{
    private readonly IPromotionRepository _promotionRepository;
    private readonly IValidator<NotifyPromotionRequest> _validator;
    private readonly ILoggedUserService _loggedUserService;

    /// <summary>
    /// Inicializa uma nova instância do <see cref="NotifyPromotionUseCase"/>.
    /// </summary>
    /// <param name="promotionRepository">Repositório de promoções.</param>
    /// <param name="validator">Validador para a requisição.</param>
    /// <param name="loggedUserService">Serviço para obter informações do usuário logado.</param>
    /// <param name="logger">Logger.</param>
    /// <param name="exceptionHandler">Serviço de tratamento de exceções.</param>
    public NotifyPromotionUseCase(
        IPromotionRepository promotionRepository,
        IValidator<NotifyPromotionRequest> validator,
        ILoggedUserService loggedUserService,
        ILogger<NotifyPromotionUseCase> logger,
        IExceptionHandlerService exceptionHandler)
        : base(logger, exceptionHandler)
    {
        _promotionRepository = promotionRepository;
        _validator = validator;
        _loggedUserService = loggedUserService;
    }

    /// <summary>
    /// Executa a notificação de uma promoção.
    /// </summary>
    /// <param name="request">Dados da notificação.</param>
    /// <param name="user">Usuário autenticado.</param>
    public async Task ExecuteAsync(NotifyPromotionRequest request, ClaimsPrincipal user)
    {
        await ExecuteWithExceptionHandlingAsync(async () =>
        {
            var tenantId = _loggedUserService.GetTenantId(user);
            
            // Validação dos dados de entrada
            await ValidateRequestAsync(request);

            // Busca e validação da promoção
            var promotion = await GetAndValidatePromotionAsync(request.PromotionId, tenantId);

            // Envio da notificação
            await SendNotificationAsync(promotion, request);
        });
    }

    /// <summary>
    /// Valida os dados da requisição.
    /// </summary>
    /// <param name="request">Requisição a ser validada.</param>
    private async Task ValidateRequestAsync(NotifyPromotionRequest request)
    {
        var validationResult = await _validator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult);
        }
    }

    /// <summary>
    /// Busca e valida a promoção.
    /// </summary>
    /// <param name="promotionId">ID da promoção.</param>
    /// <param name="tenantId">ID do tenant.</param>
    /// <returns>Promoção encontrada.</returns>
    private async Task<Domain.Entities.Promotion> GetAndValidatePromotionAsync(string promotionId, string tenantId)
    {
        var promotion = await _promotionRepository.GetByIdAsync(promotionId, tenantId);
        EnsureEntityExists(promotion, "Promoção", promotionId);
        return promotion!; // Garantido que não é null após EnsureEntityExists
    }

    /// <summary>
    /// Envia a notificação da promoção.
    /// </summary>
    /// <param name="promotion">Promoção a ser notificada.</param>
    /// <param name="request">Dados da notificação.</param>
    private Task SendNotificationAsync(Domain.Entities.Promotion promotion, NotifyPromotionRequest request)
    {
        // Placeholder: futura integração com API WhatsApp
        // Exemplo: await _whatsappService.SendMessageAsync(promotion, request.MessageTemplate);
        
        // Por enquanto, apenas valida que a promoção existe e está ativa
        ValidateBusinessRule(promotion.IsActive, "Promoção deve estar ativa para ser notificada.", "PROMOTION_ACTIVE_RULE");
        return Task.CompletedTask; // Adicionado para retornar uma Task
    }
}