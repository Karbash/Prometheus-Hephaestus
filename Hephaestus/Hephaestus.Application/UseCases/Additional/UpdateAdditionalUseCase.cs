using FluentValidation;
using Hephaestus.Domain.DTOs.Request;
using Hephaestus.Application.Interfaces.Additional;
using Hephaestus.Domain.Repositories;
using Hephaestus.Application.Exceptions;
using Hephaestus.Application.Base;
using Microsoft.Extensions.Logging;
using Hephaestus.Application.Services;
using Hephaestus.Domain.Services;
using System.Security.Claims;
using FluentValidation.Results;

namespace Hephaestus.Application.UseCases.Additional;

/// <summary>
/// Caso de uso para atualiza��o de adicionais.
/// </summary>
public class UpdateAdditionalUseCase : BaseUseCase, IUpdateAdditionalUseCase
{
    private readonly IAdditionalRepository _additionalRepository;
    private readonly IValidator<UpdateAdditionalRequest> _validator;
    private readonly ILoggedUserService _loggedUserService;

    /// <summary>
    /// Inicializa uma nova inst�ncia do <see cref="UpdateAdditionalUseCase"/>.
    /// </summary>
    /// <param name="additionalRepository">Reposit�rio de adicionais.</param>
    /// <param name="validator">Validador para a requisi��o.</param>
    /// <param name="logger">Logger.</param>
    /// <param name="exceptionHandler">Servi�o de tratamento de exce��es.</param>
    /// <param name="loggedUserService">Servi�o do usu�rio logado.</param>
    public UpdateAdditionalUseCase(
        IAdditionalRepository additionalRepository,
        IValidator<UpdateAdditionalRequest> validator,
        ILogger<UpdateAdditionalUseCase> logger,
        IExceptionHandlerService exceptionHandler,
        ILoggedUserService loggedUserService)
        : base(logger, exceptionHandler)
    {
        _additionalRepository = additionalRepository;
        _validator = validator;
        _loggedUserService = loggedUserService;
    }

    /// <summary>
    /// Executa a atualiza��o de um adicional.
    /// </summary>
    /// <param name="id">ID do adicional.</param>
    /// <param name="request">Dados atualizados do adicional.</param>
    /// <param name="user">Usu�rio autenticado.</param>
    public async Task ExecuteAsync(string id, UpdateAdditionalRequest request, ClaimsPrincipal user)
    {
        await ExecuteWithExceptionHandlingAsync(async () =>
        {
            // Obter tenantId do usu�rio logado
            var tenantId = _loggedUserService.GetTenantId(user);

            // Valida��o dos dados de entrada
            await ValidateRequestAsync(request, id);

            // Busca e valida��o do adicional
            var additional = await GetAndValidateAdditionalAsync(id, tenantId);

            // Atualiza��o do adicional
            await UpdateAdditionalAsync(additional, request);
        });
    }

    /// <summary>
    /// Valida os dados da requisi��o.
    /// </summary>
    /// <param name="request">Requisi��o a ser validada.</param>
    /// <param name="id">ID do adicional.</param>
    private async Task ValidateRequestAsync(UpdateAdditionalRequest request, string id)
    {
        var validationResult = await _validator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            throw new Hephaestus.Application.Exceptions.ValidationException("Dados do adicional inv�lidos", validationResult);
        }
    }

    /// <summary>
    /// Busca e valida o adicional.
    /// </summary>
    /// <param name="id">ID do adicional.</param>
    /// <param name="tenantId">ID do tenant.</param>
    /// <returns>Adicional encontrado.</returns>
    private async Task<Domain.Entities.Additional> GetAndValidateAdditionalAsync(string id, string tenantId)
    {
        var additional = await _additionalRepository.GetByIdAsync(id, tenantId);
        EnsureEntityExists(additional, "Adicional", id);
        return additional!; // Garantido que n�o � null ap�s EnsureEntityExists
    }

    /// <summary>
    /// Atualiza o adicional com os novos dados.
    /// </summary>
    /// <param name="additional">Adicional a ser atualizado.</param>
    /// <param name="request">Dados atualizados.</param>
    private async Task UpdateAdditionalAsync(Domain.Entities.Additional additional, UpdateAdditionalRequest request)
    {
        additional.Name = request.Name ?? additional.Name;
        additional.Price = request.Price ?? additional.Price;

        await _additionalRepository.UpdateAsync(additional);
    }
}
