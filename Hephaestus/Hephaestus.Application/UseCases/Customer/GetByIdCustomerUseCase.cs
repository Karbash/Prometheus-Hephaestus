using Hephaestus.Domain.DTOs.Response;
using Hephaestus.Application.Interfaces.Customer;
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
/// Caso de uso para obter um cliente espec�fico por ID.
/// </summary>
public class GetByIdCustomerUseCase : BaseUseCase, IGetByIdCustomerUseCase
{
    private readonly ICustomerRepository _customerRepository;
    private readonly ICompanyRepository _companyRepository;
    private readonly ILoggedUserService _loggedUserService;
    private readonly IAddressRepository _addressRepository;

    /// <summary>
    /// Inicializa uma nova inst�ncia do <see cref="GetByIdCustomerUseCase"/>.
    /// </summary>
    /// <param name="customerRepository">Reposit�rio de clientes.</param>
    /// <param name="companyRepository">Reposit�rio de empresas.</param>
    /// <param name="loggedUserService">Servi�o para obter informa��es do usu�rio logado.</param>
    /// <param name="logger">Logger.</param>
    /// <param name="exceptionHandler">Servi�o de tratamento de exce��es.</param>
    public GetByIdCustomerUseCase(
        ICustomerRepository customerRepository, 
        ICompanyRepository companyRepository,
        ILoggedUserService loggedUserService,
        IAddressRepository addressRepository,
        ILogger<GetByIdCustomerUseCase> logger,
        IExceptionHandlerService exceptionHandler)
        : base(logger, exceptionHandler)
    {
        _customerRepository = customerRepository;
        _companyRepository = companyRepository;
        _loggedUserService = loggedUserService;
        _addressRepository = addressRepository;
    }

    /// <summary>
    /// Executa a busca de um cliente espec�fico por ID.
    /// </summary>
    /// <param name="id">ID do cliente.</param>
    /// <param name="user">Usu�rio autenticado.</param>
    /// <returns>Cliente encontrado ou null se n�o existir.</returns>
    public async Task<CustomerResponse?> GetByIdAsync(string id, ClaimsPrincipal user)
    {
        return await ExecuteWithExceptionHandlingAsync(async () =>
        {
            var tenantId = _loggedUserService.GetTenantId(user);
            
            // Valida��o dos par�metros de entrada
            ValidateInputParameters(id, tenantId);

            // Valida��o do tenant
            await ValidateTenantAsync(tenantId);

            // Busca do cliente
            var customer = await GetAndValidateCustomerAsync(id, tenantId);

            // Convers�o para DTO de resposta
            return customer != null ? await ConvertToResponseDtoAsync(customer) : null;
        });
    }

    /// <summary>
    /// Valida os par�metros de entrada.
    /// </summary>
    /// <param name="id">ID do cliente.</param>
    /// <param name="tenantId">ID do tenant.</param>
    private void ValidateInputParameters(string id, string tenantId)
    {
        if (string.IsNullOrEmpty(id))
            throw new Hephaestus.Application.Exceptions.ValidationException("ID do cliente � obrigat�rio.", new ValidationResult());

        if (string.IsNullOrEmpty(tenantId))
            throw new Hephaestus.Application.Exceptions.ValidationException("ID do tenant � obrigat�rio.", new ValidationResult());

        if (!Guid.TryParse(id, out _))
            throw new Hephaestus.Application.Exceptions.ValidationException("ID do cliente deve ser um GUID v�lido.", new ValidationResult());
    }

    /// <summary>
    /// Valida o tenant.
    /// </summary>
    /// <param name="tenantId">ID do tenant.</param>
    private async Task ValidateTenantAsync(string tenantId)
    {
        var company = await _companyRepository.GetByIdAsync(tenantId);
        if (company == null)
            throw new NotFoundException("Tenant", tenantId);

        if (company.Role.ToString() != "Tenant")
            throw new BusinessRuleException("Apenas tenants podem acessar clientes.", "TENANT_ROLE_VALIDATION");
    }

    /// <summary>
    /// Busca e valida o cliente.
    /// </summary>
    /// <param name="id">ID do cliente.</param>
    /// <param name="tenantId">ID do tenant.</param>
    /// <returns>Cliente encontrado.</returns>
    private async Task<Domain.Entities.Customer> GetAndValidateCustomerAsync(string id, string tenantId)
    {
        var customer = await _customerRepository.GetByIdAsync(id, tenantId);
        if (customer == null)
            throw new NotFoundException("Cliente", id);

        return customer;
    }

    /// <summary>
    /// Converte a entidade para DTO de resposta.
    /// </summary>
    /// <param name="customer">Cliente encontrado.</param>
    /// <returns>DTO de resposta.</returns>
    private async Task<CustomerResponse> ConvertToResponseDtoAsync(Domain.Entities.Customer customer)
    {
        var address = (await _addressRepository.GetByEntityAsync(customer.Id, "Customer")).FirstOrDefault();
        return new CustomerResponse
        {
            Id = customer.Id,
            TenantId = customer.TenantId,
            PhoneNumber = customer.PhoneNumber,
            Name = customer.Name,
            DietaryPreferences = customer.DietaryPreferences,
            PreferredPaymentMethod = customer.PreferredPaymentMethod,
            NotificationPreferences = customer.NotificationPreferences ?? "email,sms",
            CreatedAt = customer.CreatedAt,
            Address = address != null ? new AddressResponse
            {
                Id = address.Id,
                Street = address.Street,
                Number = address.Number,
                Complement = address.Complement,
                Neighborhood = address.Neighborhood,
                City = address.City,
                State = address.State,
                ZipCode = address.ZipCode,
                Reference = address.Reference,
                Notes = address.Notes,
                Latitude = address.Latitude,
                Longitude = address.Longitude
            } : null
        };
    }
}
