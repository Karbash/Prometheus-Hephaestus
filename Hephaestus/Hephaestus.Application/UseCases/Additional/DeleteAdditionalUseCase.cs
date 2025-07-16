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
/// Caso de uso para exclus�o de adicionais.
/// </summary>
public class DeleteAdditionalUseCase : BaseUseCase, IDeleteAdditionalUseCase
{
    private readonly IAdditionalRepository _additionalRepository;
    private readonly ILoggedUserService _loggedUserService;

    /// <summary>
    /// Inicializa uma nova inst�ncia do <see cref="DeleteAdditionalUseCase"/>.
    /// </summary>
    /// <param name="additionalRepository">Reposit�rio de adicionais.</param>
    /// <param name="logger">Logger.</param>
    /// <param name="exceptionHandler">Servi�o de tratamento de exce��es.</param>
    /// <param name="loggedUserService">Servi�o do usu�rio logado.</param>
    public DeleteAdditionalUseCase(
        IAdditionalRepository additionalRepository,
        ILogger<DeleteAdditionalUseCase> logger,
        IExceptionHandlerService exceptionHandler,
        ILoggedUserService loggedUserService)
        : base(logger, exceptionHandler)
    {
        _additionalRepository = additionalRepository;
        _loggedUserService = loggedUserService;
    }

    /// <summary>
    /// Executa a exclus�o de um adicional.
    /// </summary>
    /// <param name="id">ID do adicional a ser exclu�do.</param>
    /// <param name="user">Usu�rio autenticado.</param>
    public async Task ExecuteAsync(string id, ClaimsPrincipal user)
    {
        await ExecuteWithExceptionHandlingAsync(async () =>
        {
            // Obter tenantId do usu�rio logado
            var tenantId = _loggedUserService.GetTenantId(user);

            // Valida��o dos dados de entrada
            ValidateInputParameters(id, tenantId);

            // Busca e valida��o do adicional
            var additional = await GetAndValidateAdditionalAsync(id, tenantId);

            // Exclus�o do adicional
            await DeleteAdditionalAsync(id, tenantId);
        });
    }

    /// <summary>
    /// Valida os par�metros de entrada.
    /// </summary>
    /// <param name="id">ID do adicional.</param>
    /// <param name="tenantId">ID do tenant.</param>
    private void ValidateInputParameters(string id, string tenantId)
    {
        if (string.IsNullOrEmpty(id))
            throw new Hephaestus.Application.Exceptions.ValidationException("ID do adicional � obrigat�rio.", new ValidationResult());

        if (string.IsNullOrEmpty(tenantId))
            throw new Hephaestus.Application.Exceptions.ValidationException("ID do tenant � obrigat�rio.", new ValidationResult());

        if (!Guid.TryParse(id, out _))
            throw new Hephaestus.Application.Exceptions.ValidationException("ID do adicional deve ser um GUID v�lido.", new ValidationResult());
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
    /// Exclui o adicional do reposit�rio.
    /// </summary>
    /// <param name="id">ID do adicional.</param>
    /// <param name="tenantId">ID do tenant.</param>
    private async Task DeleteAdditionalAsync(string id, string tenantId)
    {
        await _additionalRepository.DeleteAsync(id, tenantId);
    }
}
