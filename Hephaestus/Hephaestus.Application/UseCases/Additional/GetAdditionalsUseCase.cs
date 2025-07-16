using Hephaestus.Domain.DTOs.Response;
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
/// Caso de uso para obter todos os adicionais de um tenant.
/// </summary>
public class GetAdditionalsUseCase : BaseUseCase, IGetAdditionalsUseCase
{
    private readonly IAdditionalRepository _additionalRepository;
    private readonly ILoggedUserService _loggedUserService;

    /// <summary>
    /// Inicializa uma nova instância do <see cref="GetAdditionalsUseCase"/>.
    /// </summary>
    /// <param name="additionalRepository">Repositório de adicionais.</param>
    /// <param name="logger">Logger.</param>
    /// <param name="exceptionHandler">Serviço de tratamento de exceções.</param>
    /// <param name="loggedUserService">Serviço do usuário logado.</param>
    public GetAdditionalsUseCase(
        IAdditionalRepository additionalRepository,
        ILogger<GetAdditionalsUseCase> logger,
        IExceptionHandlerService exceptionHandler,
        ILoggedUserService loggedUserService)
        : base(logger, exceptionHandler)
    {
        _additionalRepository = additionalRepository;
        _loggedUserService = loggedUserService;
    }

    /// <summary>
    /// Executa a busca de todos os adicionais de um tenant.
    /// </summary>
    /// <param name="user">Usuário autenticado.</param>
    /// <returns>Lista de adicionais.</returns>
    public async Task<PagedResult<AdditionalResponse>> ExecuteAsync(ClaimsPrincipal user, int pageNumber = 1, int pageSize = 20, string? sortBy = null, string? sortOrder = "asc")
    {
        return await ExecuteWithExceptionHandlingAsync(async () =>
        {
            var tenantId = _loggedUserService.GetTenantId(user);
            ValidateInputParameters(tenantId);
            var pagedAdditionals = await _additionalRepository.GetByTenantIdAsync(tenantId, pageNumber, pageSize, sortBy, sortOrder);
            return new PagedResult<AdditionalResponse>
            {
                Items = ConvertToResponseDtos(pagedAdditionals.Items).ToList(),
                TotalCount = pagedAdditionals.TotalCount,
                PageNumber = pagedAdditionals.PageNumber,
                PageSize = pagedAdditionals.PageSize
            };
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
    private async Task<PagedResult<Domain.Entities.Additional>> GetAdditionalsAsync(string tenantId, int pageNumber = 1, int pageSize = 20, string? sortBy = null, string? sortOrder = "asc")
    {
        return await _additionalRepository.GetByTenantIdAsync(tenantId, pageNumber, pageSize, sortBy, sortOrder);
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
