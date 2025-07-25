using FluentValidation;
using FluentValidation.Results;
using Hephaestus.Domain.DTOs.Request;
using Hephaestus.Application.Interfaces.Additional;
using Hephaestus.Domain.Repositories;
using Hephaestus.Application.Exceptions;
using Hephaestus.Application.Base;
using Microsoft.Extensions.Logging;
using Hephaestus.Application.Services;
using Hephaestus.Domain.Services;
using System.Security.Claims;
using ValidationException = Hephaestus.Application.Exceptions.ValidationException;

namespace Hephaestus.Application.UseCases.Additional;

/// <summary>
/// Caso de uso para cria��o de adicionais.
/// </summary>
public class CreateAdditionalUseCase : BaseUseCase, ICreateAdditionalUseCase
{
    private readonly IAdditionalRepository _additionalRepository;
    private readonly IValidator<CreateAdditionalRequest> _validator;
    private readonly ILoggedUserService _loggedUserService;

    /// <summary>
    /// Inicializa uma nova inst�ncia do <see cref="CreateAdditionalUseCase"/>.
    /// </summary>
    /// <param name="additionalRepository">Reposit�rio de adicionais.</param>
    /// <param name="validator">Validador para a requisi��o.</param>
    /// <param name="logger">Logger.</param>
    /// <param name="exceptionHandler">Servi�o de tratamento de exce��es.</param>
    /// <param name="loggedUserService">Servi�o do usu�rio logado.</param>
    public CreateAdditionalUseCase(
        IAdditionalRepository additionalRepository,
        IValidator<CreateAdditionalRequest> validator,
        ILogger<CreateAdditionalUseCase> logger,
        IExceptionHandlerService exceptionHandler,
        ILoggedUserService loggedUserService)
        : base(logger, exceptionHandler)
    {
        _additionalRepository = additionalRepository;
        _validator = validator;
        _loggedUserService = loggedUserService;
    }

    /// <summary>
    /// Executa a cria��o de um adicional.
    /// </summary>
    /// <param name="request">Dados do adicional.</param>
    /// <param name="user">Usu�rio autenticado.</param>
    /// <returns>ID do adicional criado.</returns>
    public async Task<string> ExecuteAsync(CreateAdditionalRequest request, ClaimsPrincipal user)
    {
        return await ExecuteWithExceptionHandlingAsync(async () =>
        {
            // Valida��o dos dados de entrada
            await ValidateRequestAsync(request);

            // Obter tenantId do usu�rio logado
            var tenantId = _loggedUserService.GetTenantId(user);

            // Cria��o do adicional
            var additional = await CreateAdditionalEntityAsync(request, tenantId);

            return additional.Id;
        });
    }

    /// <summary>
    /// Valida os dados da requisi��o.
    /// </summary>
    /// <param name="request">Requisi��o a ser validada.</param>
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
