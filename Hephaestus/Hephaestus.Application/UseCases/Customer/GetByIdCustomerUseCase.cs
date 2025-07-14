using Hephaestus.Application.DTOs.Response;
using Hephaestus.Application.Interfaces.Customer;
using Hephaestus.Domain.Repositories;
using Hephaestus.Application.Exceptions;
using Hephaestus.Application.Base;
using Microsoft.Extensions.Logging;
using Hephaestus.Application.Services;
using FluentValidation.Results;

namespace Hephaestus.Application.UseCases.Customer;

/// <summary>
/// Caso de uso para obter um cliente específico por ID.
/// </summary>
public class GetByIdCustomerUseCase : BaseUseCase, IGetByIdCustomerUseCase
{
    private readonly ICustomerRepository _customerRepository;
    private readonly ICompanyRepository _companyRepository;

    /// <summary>
    /// Inicializa uma nova instância do <see cref="GetByIdCustomerUseCase"/>.
    /// </summary>
    /// <param name="customerRepository">Repositório de clientes.</param>
    /// <param name="companyRepository">Repositório de empresas.</param>
    /// <param name="logger">Logger.</param>
    /// <param name="exceptionHandler">Serviço de tratamento de exceções.</param>
    public GetByIdCustomerUseCase(
        ICustomerRepository customerRepository, 
        ICompanyRepository companyRepository,
        ILogger<GetByIdCustomerUseCase> logger,
        IExceptionHandlerService exceptionHandler)
        : base(logger, exceptionHandler)
    {
        _customerRepository = customerRepository;
        _companyRepository = companyRepository;
    }

    /// <summary>
    /// Executa a busca de um cliente específico por ID.
    /// </summary>
    /// <param name="id">ID do cliente.</param>
    /// <param name="tenantId">ID do tenant.</param>
    /// <returns>Cliente encontrado ou null se não existir.</returns>
    public async Task<CustomerResponse?> GetByIdAsync(string id, string tenantId)
    {
        return await ExecuteWithExceptionHandlingAsync(async () =>
        {
            // Validação dos parâmetros de entrada
            ValidateInputParameters(id, tenantId);

            // Validação do tenant
            await ValidateTenantAsync(tenantId);

            // Busca do cliente
            var customer = await GetAndValidateCustomerAsync(id, tenantId);

            // Conversão para DTO de resposta
            return customer != null ? ConvertToResponseDto(customer) : null;
        });
    }

    /// <summary>
    /// Valida os parâmetros de entrada.
    /// </summary>
    /// <param name="id">ID do cliente.</param>
    /// <param name="tenantId">ID do tenant.</param>
    private void ValidateInputParameters(string id, string tenantId)
    {
        if (string.IsNullOrEmpty(id))
            throw new Hephaestus.Application.Exceptions.ValidationException("ID do cliente é obrigatório.", new ValidationResult());

        if (string.IsNullOrEmpty(tenantId))
            throw new Hephaestus.Application.Exceptions.ValidationException("ID do tenant é obrigatório.", new ValidationResult());

        if (!Guid.TryParse(id, out _))
            throw new Hephaestus.Application.Exceptions.ValidationException("ID do cliente deve ser um GUID válido.", new ValidationResult());
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
    private CustomerResponse ConvertToResponseDto(Domain.Entities.Customer customer)
    {
        return new CustomerResponse
        {
            Id = customer.Id,
            TenantId = customer.TenantId,
            PhoneNumber = customer.PhoneNumber,
            Name = customer.Name,
            State = customer.State,
            City = customer.City,
            Street = customer.Street,
            Number = customer.Number,
            Latitude = customer.Latitude,
            Longitude = customer.Longitude,
            CreatedAt = customer.CreatedAt
        };
    }
}