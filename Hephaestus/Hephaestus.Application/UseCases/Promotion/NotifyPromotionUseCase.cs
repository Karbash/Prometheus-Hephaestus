using FluentValidation;
using FluentValidation.Results;
using Hephaestus.Domain.DTOs.Request;
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
/// Caso de uso para notifica��o de promo��es.
/// </summary>
public class NotifyPromotionUseCase : BaseUseCase, INotifyPromotionUseCase
{
    private readonly IPromotionRepository _promotionRepository;
    private readonly IValidator<NotifyPromotionRequest> _validator;
    private readonly ILoggedUserService _loggedUserService;

    /// <summary>
    /// Inicializa uma nova inst�ncia do <see cref="NotifyPromotionUseCase"/>.
    /// </summary>
    /// <param name="promotionRepository">Reposit�rio de promo��es.</param>
    /// <param name="validator">Validador para a requisi��o.</param>
    /// <param name="loggedUserService">Servi�o para obter informa��es do usu�rio logado.</param>
    /// <param name="logger">Logger.</param>
    /// <param name="exceptionHandler">Servi�o de tratamento de exce��es.</param>
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
    /// Executa a notifica��o de uma promo��o.
    /// </summary>
    /// <param name="request">Dados da notifica��o.</param>
    /// <param name="user">Usu�rio autenticado.</param>
    public async Task ExecuteAsync(NotifyPromotionRequest request, ClaimsPrincipal user)
    {
        await ExecuteWithExceptionHandlingAsync(async () =>
        {
            var companyId = _loggedUserService.GetCompanyId(user);
            
            // Validao dos dados de entrada
            await ValidateRequestAsync(request);

            // Busca e valida��o da promo��o
            var promotion = await GetAndValidatePromotionAsync(request.PromotionId, companyId);

            // Envio da notificao
            await SendNotificationAsync(promotion, request);
        });
    }

    /// <summary>
    /// Valida os dados da requisi��o.
    /// </summary>
    /// <param name="request">Requisi��o a ser validada.</param>
    private async Task ValidateRequestAsync(NotifyPromotionRequest request)
    {
        var validationResult = await _validator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult);
        }
    }

    /// <summary>
    /// Busca e valida a promo��o.
    /// </summary>
    /// <param name="promotionId">ID da promo��o.</param>
    /// <param name="companyId">ID da empresa.</param>
    /// <returns>Promoo encontrada.</returns>
    private async Task<Domain.Entities.Promotion> GetAndValidatePromotionAsync(string promotionId, string companyId)
    {
        var promotion = await _promotionRepository.GetByIdAsync(promotionId, companyId);
        if (promotion == null)
            throw new NotFoundException("Promotion", promotionId);
        return promotion;
    }

    /// <summary>
    /// Envia a notificao da promoo.
    /// </summary>
    /// <param name="promotion">Promo��o a ser notificada.</param>
    /// <param name="request">Dados da notifica��o.</param>
    private Task SendNotificationAsync(Domain.Entities.Promotion promotion, NotifyPromotionRequest request)
    {
        // Placeholder: futura integra��o com API WhatsApp
        // Exemplo: await _whatsappService.SendMessageAsync(promotion, request.MessageTemplate);
        
        // Por enquanto, apenas valida que a promo��o existe e est� ativa
        ValidateBusinessRule(promotion.IsActive, "Promo��o deve estar ativa para ser notificada.", "PROMOTION_ACTIVE_RULE");
        return Task.CompletedTask; // Adicionado para retornar uma Task
    }
}
