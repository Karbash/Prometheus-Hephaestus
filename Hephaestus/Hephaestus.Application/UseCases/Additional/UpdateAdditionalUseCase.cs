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
/// Caso de uso para atualização de adicionais.
/// </summary>
public class UpdateAdditionalUseCase : BaseUseCase, IUpdateAdditionalUseCase
{
    private readonly IAdditionalRepository _additionalRepository;
    private readonly IValidator<UpdateAdditionalRequest> _validator;
    private readonly ILoggedUserService _loggedUserService;

    /// <summary>
    /// Inicializa uma nova instância do <see cref="UpdateAdditionalUseCase"/>.
    /// </summary>
    /// <param name="additionalRepository">Repositório de adicionais.</param>
    /// <param name="validator">Validador para a requisição.</param>
    /// <param name="logger">Logger.</param>
    /// <param name="exceptionHandler">Serviço de tratamento de exceções.</param>
    /// <param name="loggedUserService">Serviço do usuário logado.</param>
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
    /// Executa a atualização de um adicional.
    /// </summary>
    /// <param name="id">ID do adicional.</param>
    /// <param name="request">Dados atualizados do adicional.</param>
    /// <param name="user">Usuário autenticado.</param>
    public async Task ExecuteAsync(string id, UpdateAdditionalRequest request, ClaimsPrincipal user)
    {
        await ExecuteWithExceptionHandlingAsync(async () =>
        {
            // Obter tenantId do usuário logado
            var tenantId = _loggedUserService.GetTenantId(user);

            // Validação dos dados de entrada
            await ValidateRequestAsync(request, id);

            // Busca e validação do adicional
            var additional = await GetAndValidateAdditionalAsync(id, tenantId);

            // Atualização do adicional
            await UpdateAdditionalAsync(additional, request);
        });
    }

    /// <summary>
    /// Valida os dados da requisição.
    /// </summary>
    /// <param name="request">Requisição a ser validada.</param>
    /// <param name="id">ID do adicional.</param>
    private async Task ValidateRequestAsync(UpdateAdditionalRequest request, string id)
    {
        var validationResult = await _validator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            throw new Hephaestus.Application.Exceptions.ValidationException("Dados do adicional inválidos", validationResult);
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
        return additional!; // Garantido que não é null após EnsureEntityExists
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
