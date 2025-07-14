using Hephaestus.Application.DTOs.Response;
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
/// Caso de uso para obter um adicional específico por ID.
/// </summary>
public class GetAdditionalByIdUseCase : BaseUseCase, IGetAdditionalByIdUseCase
{
    private readonly IAdditionalRepository _additionalRepository;
    private readonly ILoggedUserService _loggedUserService;

    /// <summary>
    /// Inicializa uma nova instância do <see cref="GetAdditionalByIdUseCase"/>.
    /// </summary>
    /// <param name="additionalRepository">Repositório de adicionais.</param>
    /// <param name="logger">Logger.</param>
    /// <param name="exceptionHandler">Serviço de tratamento de exceções.</param>
    /// <param name="loggedUserService">Serviço do usuário logado.</param>
    public GetAdditionalByIdUseCase(
        IAdditionalRepository additionalRepository,
        ILogger<GetAdditionalByIdUseCase> logger,
        IExceptionHandlerService exceptionHandler,
        ILoggedUserService loggedUserService)
        : base(logger, exceptionHandler)
    {
        _additionalRepository = additionalRepository;
        _loggedUserService = loggedUserService;
    }

    /// <summary>
    /// Executa a busca de um adicional específico.
    /// </summary>
    /// <param name="id">ID do adicional.</param>
    /// <param name="user">Usuário autenticado.</param>
    /// <returns>Adicional encontrado.</returns>
    public async Task<AdditionalResponse> ExecuteAsync(string id, ClaimsPrincipal user)
    {
        return await ExecuteWithExceptionHandlingAsync(async () =>
        {
            // Obter tenantId do usuário logado
            var tenantId = _loggedUserService.GetTenantId(user);

            // Validação dos parâmetros de entrada
            ValidateInputParameters(id, tenantId);

            // Busca e validação do adicional
            var additional = await GetAndValidateAdditionalAsync(id, tenantId);

            // Conversão para DTO de resposta
            return ConvertToResponseDto(additional);
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
    /// Converte a entidade para DTO de resposta.
    /// </summary>
    /// <param name="additional">Adicional encontrado.</param>
    /// <returns>DTO de resposta.</returns>
    private AdditionalResponse ConvertToResponseDto(Domain.Entities.Additional additional)
    {
        return new AdditionalResponse
        {
            Id = additional.Id,
            TenantId = additional.TenantId,
            Name = additional.Name,
            Price = additional.Price
        };
    }
}