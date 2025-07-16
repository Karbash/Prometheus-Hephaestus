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
/// Caso de uso para exclusão de adicionais.
/// </summary>
public class DeleteAdditionalUseCase : BaseUseCase, IDeleteAdditionalUseCase
{
    private readonly IAdditionalRepository _additionalRepository;
    private readonly ILoggedUserService _loggedUserService;

    /// <summary>
    /// Inicializa uma nova instância do <see cref="DeleteAdditionalUseCase"/>.
    /// </summary>
    /// <param name="additionalRepository">Repositório de adicionais.</param>
    /// <param name="logger">Logger.</param>
    /// <param name="exceptionHandler">Serviço de tratamento de exceções.</param>
    /// <param name="loggedUserService">Serviço do usuário logado.</param>
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
    /// Executa a exclusão de um adicional.
    /// </summary>
    /// <param name="id">ID do adicional a ser excluído.</param>
    /// <param name="user">Usuário autenticado.</param>
    public async Task ExecuteAsync(string id, ClaimsPrincipal user)
    {
        await ExecuteWithExceptionHandlingAsync(async () =>
        {
            // Obter tenantId do usuário logado
            var tenantId = _loggedUserService.GetTenantId(user);

            // Validação dos dados de entrada
            ValidateInputParameters(id, tenantId);

            // Busca e validação do adicional
            var additional = await GetAndValidateAdditionalAsync(id, tenantId);

            // Exclusão do adicional
            await DeleteAdditionalAsync(id, tenantId);
        });
    }

    /// <summary>
    /// Valida os parâmetros de entrada.
    /// </summary>
    /// <param name="id">ID do adicional.</param>
    /// <param name="tenantId">ID do tenant.</param>
    private void ValidateInputParameters(string id, string tenantId)
    {
        if (string.IsNullOrEmpty(id))
            throw new Hephaestus.Application.Exceptions.ValidationException("ID do adicional é obrigatório.", new ValidationResult());

        if (string.IsNullOrEmpty(tenantId))
            throw new Hephaestus.Application.Exceptions.ValidationException("ID do tenant é obrigatório.", new ValidationResult());

        if (!Guid.TryParse(id, out _))
            throw new Hephaestus.Application.Exceptions.ValidationException("ID do adicional deve ser um GUID válido.", new ValidationResult());
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
        return additional!; // Garantido que não é null após EnsureEntityExists
    }

    /// <summary>
    /// Exclui o adicional do repositório.
    /// </summary>
    /// <param name="id">ID do adicional.</param>
    /// <param name="tenantId">ID do tenant.</param>
    private async Task DeleteAdditionalAsync(string id, string tenantId)
    {
        await _additionalRepository.DeleteAsync(id, tenantId);
    }
}
