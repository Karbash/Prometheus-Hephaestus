using FluentValidation.Results;
using Hephaestus.Application.Base;
using Hephaestus.Domain.DTOs.Response;
using Hephaestus.Application.Exceptions;
using Hephaestus.Application.Interfaces.Customer;
using Hephaestus.Application.Services;
using Hephaestus.Domain.Repositories;
using Hephaestus.Domain.Services;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using Hephaestus.Domain.DTOs.Response;

namespace Hephaestus.Application.UseCases.Customer;

/// <summary>
/// Caso de uso para obter clientes de um tenant.
/// </summary>
public class GetCustomerUseCase : BaseUseCase, IGetCustomerUseCase
{
    private readonly ICustomerRepository _customerRepository;
    private readonly ICompanyRepository _companyRepository;
    private readonly ILoggedUserService _loggedUserService;

    /// <summary>
    /// Inicializa uma nova inst�ncia do <see cref="GetCustomerUseCase"/>.
    /// </summary>
    /// <param name="customerRepository">Reposit�rio de clientes.</param>
    /// <param name="companyRepository">Reposit�rio de empresas.</param>
    /// <param name="loggedUserService">Servi�o para obter informa��es do usu�rio logado.</param>
    /// <param name="logger">Logger.</param>
    /// <param name="exceptionHandler">Servi�o de tratamento de exce��es.</param>
    public GetCustomerUseCase(
        ICustomerRepository customerRepository, 
        ICompanyRepository companyRepository,
        ILoggedUserService loggedUserService,
        ILogger<GetCustomerUseCase> logger,
        IExceptionHandlerService exceptionHandler)
        : base(logger, exceptionHandler)
    {
        _customerRepository = customerRepository;
        _companyRepository = companyRepository;
        _loggedUserService = loggedUserService;
    }

    /// <summary>
    /// Executa a busca de clientes de um tenant.
    /// </summary>
    /// <param name="phoneNumber">N�mero de telefone para filtrar (opcional).</param>
    /// <param name="user">Usu�rio autenticado.</param>
    /// <param name="pageNumber">N�mero da p�gina.</param>
    /// <param name="pageSize">Tamanho da p�gina.</param>
    /// <returns>Lista de clientes.</returns>
    public async Task<PagedResult<CustomerResponse>> ExecuteAsync(string? phoneNumber, ClaimsPrincipal user, int pageNumber = 1, int pageSize = 20, string? sortBy = null, string? sortOrder = "asc")
    {
        return await ExecuteWithExceptionHandlingAsync(async () =>
        {
            var tenantId = _loggedUserService.GetTenantId(user);
            var pagedCustomers = await _customerRepository.GetAllAsync(phoneNumber, tenantId, pageNumber, pageSize, sortBy, sortOrder);
            return new PagedResult<CustomerResponse>
            {
                Items = pagedCustomers.Items.Select(c => new CustomerResponse
                {
                    Id = c.Id,
                    TenantId = c.TenantId,
                    PhoneNumber = c.PhoneNumber,
                    Name = c.Name,
                    CreatedAt = c.CreatedAt
                }).ToList(),
                TotalCount = pagedCustomers.TotalCount,
                PageNumber = pagedCustomers.PageNumber,
                PageSize = pagedCustomers.PageSize
            };
        });
    }

    /// <summary>
    /// Valida os par�metros de entrada.
    /// </summary>
    /// <param name="phoneNumber">N�mero de telefone.</param>
    /// <param name="tenantId">ID do tenant.</param>
    private void ValidateInputParameters(string? phoneNumber, string tenantId)
    {
        if (string.IsNullOrEmpty(tenantId))
            throw new Hephaestus.Application.Exceptions.ValidationException("ID do tenant � obrigat�rio.", new ValidationResult());

        if (!string.IsNullOrEmpty(phoneNumber) && phoneNumber.Length < 10)
            throw new Hephaestus.Application.Exceptions.ValidationException("N�mero de telefone deve ter pelo menos 10 d�gitos.", new ValidationResult());
    }

    /// <summary>
    /// Valida se o tenant existe e � v�lido.
    /// </summary>
    /// <param name="tenantId">ID do tenant.</param>
    private async Task ValidateTenantAsync(string tenantId)
    {
        var company = await _companyRepository.GetByIdAsync(tenantId);
        if (company == null)
            throw new NotFoundException("Tenant", tenantId);

        if (company.Role.ToString() != "Tenant")
            throw new BusinessRuleException("Apenas tenants podem acessar clientes.", "TENANT_ACCESS_RULE");
    }

    /// <summary>
    /// Busca os clientes no reposit�rio.
    /// </summary>
    /// <param name="phoneNumber">N�mero de telefone para filtrar.</param>
    /// <param name="tenantId">ID do tenant.</param>
    /// <returns>Lista de clientes.</returns>
    private async Task<IEnumerable<Domain.Entities.Customer>> GetCustomersAsync(string? phoneNumber, string tenantId)
    {
        var pagedCustomers = await _customerRepository.GetAllAsync(phoneNumber, tenantId);
        return pagedCustomers.Items;
    }

    /// <summary>
    /// Converte as entidades para DTOs de resposta.
    /// </summary>
    /// <param name="customers">Lista de clientes.</param>
    /// <returns>Lista de DTOs de resposta.</returns>
    private IEnumerable<CustomerResponse> ConvertToResponseDtos(IEnumerable<Domain.Entities.Customer> customers)
    {
        return customers.Select(c => new CustomerResponse
        {
            Id = c.Id,
            TenantId = c.TenantId,
            PhoneNumber = c.PhoneNumber,
            Name = c.Name,
            CreatedAt = c.CreatedAt
        });
    }
}
