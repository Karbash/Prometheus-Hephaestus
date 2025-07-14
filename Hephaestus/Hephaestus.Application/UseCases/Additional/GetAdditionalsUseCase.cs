using Hephaestus.Application.DTOs.Response;
using Hephaestus.Application.Interfaces.Additional;
using Hephaestus.Domain.Repositories;
using Hephaestus.Application.Exceptions;
using Hephaestus.Application.Base;
using Microsoft.Extensions.Logging;
using Hephaestus.Application.Services;
using FluentValidation.Results;

namespace Hephaestus.Application.UseCases.Additional;

/// <summary>
/// Caso de uso para obter todos os adicionais de um tenant.
/// </summary>
public class GetAdditionalsUseCase : BaseUseCase, IGetAdditionalsUseCase
{
    private readonly IAdditionalRepository _additionalRepository;

    /// <summary>
    /// Inicializa uma nova instância do <see cref="GetAdditionalsUseCase"/>.
    /// </summary>
    /// <param name="additionalRepository">Repositório de adicionais.</param>
    /// <param name="logger">Logger.</param>
    /// <param name="exceptionHandler">Serviço de tratamento de exceções.</param>
    public GetAdditionalsUseCase(
        IAdditionalRepository additionalRepository,
        ILogger<GetAdditionalsUseCase> logger,
        IExceptionHandlerService exceptionHandler)
        : base(logger, exceptionHandler)
    {
        _additionalRepository = additionalRepository;
    }

    /// <summary>
    /// Executa a busca de todos os adicionais de um tenant.
    /// </summary>
    /// <param name="tenantId">ID do tenant.</param>
    /// <returns>Lista de adicionais.</returns>
    public async Task<IEnumerable<AdditionalResponse>> ExecuteAsync(string tenantId)
    {
        return await ExecuteWithExceptionHandlingAsync(async () =>
        {
            // Validação dos parâmetros de entrada
            ValidateInputParameters(tenantId);

            // Busca dos adicionais
            var additionals = await GetAdditionalsAsync(tenantId);

            // Conversão para DTOs de resposta
            return ConvertToResponseDtos(additionals);
        });
    }

    /// <summary>
    /// Valida os parâmetros de entrada.
    /// </summary>
    /// <param name="tenantId">ID do tenant.</param>
    private void ValidateInputParameters(string tenantId)
    {
        if (string.IsNullOrEmpty(tenantId))
            throw new Hephaestus.Application.Exceptions.ValidationException("ID do tenant é obrigatório.", new ValidationResult());
    }

    /// <summary>
    /// Busca os adicionais.
    /// </summary>
    /// <param name="tenantId">ID do tenant.</param>
    /// <returns>Lista de adicionais.</returns>
    private async Task<IEnumerable<Domain.Entities.Additional>> GetAdditionalsAsync(string tenantId)
    {
        return await _additionalRepository.GetByTenantIdAsync(tenantId);
    }

    /// <summary>
    /// Converte as entidades para DTOs de resposta.
    /// </summary>
    /// <param name="additionals">Lista de adicionais.</param>
    /// <returns>Lista de DTOs de resposta.</returns>
    private IEnumerable<AdditionalResponse> ConvertToResponseDtos(IEnumerable<Domain.Entities.Additional> additionals)
    {
        return additionals.Select(a => new AdditionalResponse
        {
            Id = a.Id,
            TenantId = a.TenantId,
            Name = a.Name,
            Price = a.Price
        }).ToList();
    }
}