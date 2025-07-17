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
/// Caso de uso para atualiza��o de clientes.
/// </summary>
public class UpdateCustomerUseCase : BaseUseCase, IUpdateCustomerUseCase
{
    private readonly ICustomerRepository _customerRepository;
    private readonly ICompanyRepository _companyRepository;
    private readonly ILoggedUserService _loggedUserService;
    private readonly IAddressRepository _addressRepository;

    /// <summary>
    /// Inicializa uma nova inst�ncia do <see cref="UpdateCustomerUseCase"/>.
    /// </summary>
    /// <param name="customerRepository">Reposit�rio de clientes.</param>
    /// <param name="companyRepository">Reposit�rio de empresas.</param>
    /// <param name="loggedUserService">Servi�o para obter informa��es do usu�rio logado.</param>
    /// <param name="logger">Logger.</param>
    /// <param name="exceptionHandler">Servi�o de tratamento de exce��es.</param>
    public UpdateCustomerUseCase(
        ICustomerRepository customerRepository, 
        ICompanyRepository companyRepository,
        ILoggedUserService loggedUserService,
        IAddressRepository addressRepository,
        ILogger<UpdateCustomerUseCase> logger,
        IExceptionHandlerService exceptionHandler)
        : base(logger, exceptionHandler)
    {
        _customerRepository = customerRepository;
        _companyRepository = companyRepository;
        _loggedUserService = loggedUserService;
        _addressRepository = addressRepository;
    }

    /// <summary>
    /// Executa a atualiza��o de um cliente.
    /// </summary>
    /// <param name="request">Dados do cliente a ser atualizado.</param>
    /// <param name="user">Usu�rio autenticado.</param>
    public async Task UpdateAsync(CustomerRequest request, ClaimsPrincipal user)
    {
        await ExecuteWithExceptionHandlingAsync(async () =>
        {
            var tenantId = _loggedUserService.GetTenantId(user);
            
            // Valida��o dos dados de entrada
            // Remover todas as linhas que usam _validator

            // Valida��o do tenant
            await ValidateTenantAsync(tenantId);

            // Busca do cliente
            var customer = await _customerRepository.GetByIdAsync(request.Id!, tenantId);
            EnsureResourceExists(customer, "Cliente", request.Id!);

            // Atualiza��o dos dados
            await CreateOrUpdateCustomerAsync(request, tenantId, customer);
        });
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
            throw new BusinessRuleException("Apenas tenants podem gerenciar clientes.", "TENANT_ROLE_VALIDATION");
    }

    /// <summary>
    /// Valida as regras de neg�cio.
    /// </summary>
    /// <param name="request">Dados do cliente.</param>
    /// <param name="tenantId">ID do tenant.</param>
    private async Task ValidateBusinessRulesAsync(CustomerRequest request, string tenantId)
    {
        // Verifica se j� existe um cliente com o mesmo n�mero de telefone
        var existingCustomer = await _customerRepository.GetByPhoneNumberAsync(request.PhoneNumber, tenantId);
        if (existingCustomer != null)
            throw new ConflictException("J� existe um cliente com este n�mero de telefone.", "Customer", "PhoneNumber", request.PhoneNumber);
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
            DietaryPreferences = request.DietaryPreferences,
            PreferredPaymentMethod = request.PreferredPaymentMethod,
            NotificationPreferences = request.NotificationPreferences ?? "email,sms",
            CreatedAt = existingCustomer?.CreatedAt ?? DateTime.UtcNow
        };

        if (existingCustomer == null)
            await _customerRepository.AddAsync(customer);
        else
            await _customerRepository.UpdateAsync(customer);

        // Endereço
        var address = new Hephaestus.Domain.Entities.Address
        {
            TenantId = tenantId,
            EntityId = customer.Id,
            EntityType = "Customer",
            Street = request.Address.Street,
            Number = request.Address.Number,
            Complement = request.Address.Complement,
            Neighborhood = request.Address.Neighborhood,
            City = request.Address.City,
            State = request.Address.State,
            ZipCode = request.Address.ZipCode,
            Reference = request.Address.Reference,
            Notes = request.Address.Notes,
            Latitude = request.Address.Latitude ?? 0,
            Longitude = request.Address.Longitude ?? 0,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        await _addressRepository.AddAsync(address);
    }
}
