using Hephaestus.Domain.DTOs.Request;
using Hephaestus.Application.Interfaces.Customer;
using Hephaestus.Domain.Entities;
using Hephaestus.Domain.Repositories;
using Hephaestus.Application.Exceptions;
using Hephaestus.Application.Base;
using Microsoft.Extensions.Logging;
using Hephaestus.Application.Services;
using Hephaestus.Domain.Services;
using FluentValidation.Results;
using System.Security.Claims;

namespace Hephaestus.Application.UseCases.Customer;

/// <summary>
/// Caso de uso para atualização de clientes.
/// </summary>
public class UpdateCustomerUseCase : BaseUseCase, IUpdateCustomerUseCase
{
    private readonly ICustomerRepository _customerRepository;
    private readonly ICompanyRepository _companyRepository;
    private readonly ILoggedUserService _loggedUserService;

    /// <summary>
    /// Inicializa uma nova instância do <see cref="UpdateCustomerUseCase"/>.
    /// </summary>
    /// <param name="customerRepository">Repositório de clientes.</param>
    /// <param name="companyRepository">Repositório de empresas.</param>
    /// <param name="loggedUserService">Serviço para obter informações do usuário logado.</param>
    /// <param name="logger">Logger.</param>
    /// <param name="exceptionHandler">Serviço de tratamento de exceções.</param>
    public UpdateCustomerUseCase(
        ICustomerRepository customerRepository, 
        ICompanyRepository companyRepository,
        ILoggedUserService loggedUserService,
        ILogger<UpdateCustomerUseCase> logger,
        IExceptionHandlerService exceptionHandler)
        : base(logger, exceptionHandler)
    {
        _customerRepository = customerRepository;
        _companyRepository = companyRepository;
        _loggedUserService = loggedUserService;
    }

    /// <summary>
    /// Executa a atualização de um cliente.
    /// </summary>
    /// <param name="request">Dados do cliente a ser atualizado.</param>
    /// <param name="user">Usuário autenticado.</param>
    public async Task UpdateAsync(CustomerRequest request, ClaimsPrincipal user)
    {
        await ExecuteWithExceptionHandlingAsync(async () =>
        {
            var tenantId = _loggedUserService.GetTenantId(user);
            
            // Validação dos dados de entrada
            // Remover todas as linhas que usam _validator

            // Validação do tenant
            await ValidateTenantAsync(tenantId);

            // Busca do cliente
            var customer = await _customerRepository.GetByIdAsync(request.Id!, tenantId);
            EnsureResourceExists(customer, "Cliente", request.Id!);

            // Atualização dos dados
            await CreateOrUpdateCustomerAsync(request, tenantId, customer);
        });
    }

    /// <summary>
    /// Valida se o tenant existe e é válido.
    /// </summary>
    /// <param name="tenantId">ID do tenant.</param>
    private async Task ValidateTenantAsync(string tenantId)
    {
        var company = await _companyRepository.GetByIdAsync(tenantId);
        if (company == null)
            throw new NotFoundException("Tenant", tenantId);

        if (company.Role.ToString() != "Tenant")
            throw new BusinessRuleException("Apenas tenants podem gerenciar clientes.", "TENANT_ROLE_VALIDATION");
    }

    /// <summary>
    /// Valida as regras de negócio.
    /// </summary>
    /// <param name="request">Dados do cliente.</param>
    /// <param name="tenantId">ID do tenant.</param>
    private async Task ValidateBusinessRulesAsync(CustomerRequest request, string tenantId)
    {
        // Verifica se já existe um cliente com o mesmo número de telefone
        var existingCustomer = await _customerRepository.GetByPhoneNumberAsync(request.PhoneNumber, tenantId);
        if (existingCustomer != null)
            throw new ConflictException("Já existe um cliente com este número de telefone.", "Customer", "PhoneNumber", request.PhoneNumber);
    }

    /// <summary>
    /// Busca o cliente existente.
    /// </summary>
    /// <param name="request">Dados do cliente.</param>
    /// <param name="tenantId">ID do tenant.</param>
    /// <returns>Cliente existente ou null.</returns>
    private async Task<Domain.Entities.Customer?> GetExistingCustomerAsync(CustomerRequest request, string tenantId)
    {
        return await _customerRepository.GetByPhoneNumberAsync(request.PhoneNumber, tenantId);
    }

    /// <summary>
    /// Cria ou atualiza o cliente.
    /// </summary>
    /// <param name="request">Dados do cliente.</param>
    /// <param name="tenantId">ID do tenant.</param>
    /// <param name="existingCustomer">Cliente existente.</param>
    private async Task CreateOrUpdateCustomerAsync(CustomerRequest request, string tenantId, Domain.Entities.Customer? existingCustomer)
    {
        var customer = new Domain.Entities.Customer
        {
            Id = existingCustomer?.Id ?? Guid.NewGuid().ToString(),
            TenantId = tenantId,
            PhoneNumber = request.PhoneNumber,
            Name = request.Name,
            State = request.State,
            City = request.City,
            Street = request.Street,
            Number = request.Number,
            Latitude = request.Latitude,
            Longitude = request.Longitude,
            CreatedAt = existingCustomer?.CreatedAt ?? DateTime.UtcNow
        };

        if (existingCustomer == null)
            await _customerRepository.AddAsync(customer);
        else
            await _customerRepository.UpdateAsync(customer);
    }
}
