using FluentValidation;
using FluentValidation.Results;
using Hephaestus.Application.DTOs.Request;
using Hephaestus.Application.Interfaces.Additional;
using Hephaestus.Domain.Repositories;
using Hephaestus.Application.Exceptions;
using Hephaestus.Application.Base;
using Microsoft.Extensions.Logging;
using Hephaestus.Application.Services;
using ValidationException = Hephaestus.Application.Exceptions.ValidationException;

namespace Hephaestus.Application.UseCases.Additional;

/// <summary>
/// Caso de uso para criação de adicionais.
/// </summary>
public class CreateAdditionalUseCase : BaseUseCase, ICreateAdditionalUseCase
{
    private readonly IAdditionalRepository _additionalRepository;
    private readonly IValidator<CreateAdditionalRequest> _validator;

    /// <summary>
    /// Inicializa uma nova instância do <see cref="CreateAdditionalUseCase"/>.
    /// </summary>
    /// <param name="additionalRepository">Repositório de adicionais.</param>
    /// <param name="validator">Validador para a requisição.</param>
    /// <param name="logger">Logger.</param>
    /// <param name="exceptionHandler">Serviço de tratamento de exceções.</param>
    public CreateAdditionalUseCase(
        IAdditionalRepository additionalRepository,
        IValidator<CreateAdditionalRequest> validator,
        ILogger<CreateAdditionalUseCase> logger,
        IExceptionHandlerService exceptionHandler)
        : base(logger, exceptionHandler)
    {
        _additionalRepository = additionalRepository;
        _validator = validator;
    }

    /// <summary>
    /// Executa a criação de um adicional.
    /// </summary>
    /// <param name="request">Dados do adicional.</param>
    /// <param name="tenantId">ID do tenant.</param>
    /// <returns>ID do adicional criado.</returns>
    public async Task<string> ExecuteAsync(CreateAdditionalRequest request, string tenantId)
    {
        return await ExecuteWithExceptionHandlingAsync(async () =>
        {
            // Validação dos dados de entrada
            await ValidateRequestAsync(request);

            // Criação do adicional
            var additional = await CreateAdditionalEntityAsync(request, tenantId);

            return additional.Id;
        });
    }

    /// <summary>
    /// Valida os dados da requisição.
    /// </summary>
    /// <param name="request">Requisição a ser validada.</param>
    private async Task ValidateRequestAsync(CreateAdditionalRequest request)
    {
        var validationResult = await _validator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult);
        }
    }

    /// <summary>
    /// Cria a entidade de adicional.
    /// </summary>
    /// <param name="request">Dados do adicional.</param>
    /// <param name="tenantId">ID do tenant.</param>
    /// <returns>Adicional criado.</returns>
    private async Task<Domain.Entities.Additional> CreateAdditionalEntityAsync(CreateAdditionalRequest request, string tenantId)
    {
        var additional = new Domain.Entities.Additional
        {
            Id = Guid.NewGuid().ToString(),
            TenantId = tenantId,
            Name = request.Name,
            Price = request.Price
        };

        await _additionalRepository.AddAsync(additional);
        return additional;
    }
}