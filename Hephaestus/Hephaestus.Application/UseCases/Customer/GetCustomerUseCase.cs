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
/// Caso de uso para obter clientes de um tenant.
/// </summary>
public class GetCustomerUseCase : BaseUseCase, IGetCustomerUseCase
{
    private readonly ICustomerRepository _customerRepository;
    private readonly ICompanyRepository _companyRepository;

    /// <summary>
    /// Inicializa uma nova instância do <see cref="GetCustomerUseCase"/>.
    /// </summary>
    /// <param name="customerRepository">Repositório de clientes.</param>
    /// <param name="companyRepository">Repositório de empresas.</param>
    /// <param name="logger">Logger.</param>
    /// <param name="exceptionHandler">Serviço de tratamento de exceções.</param>
    public GetCustomerUseCase(
        ICustomerRepository customerRepository, 
        ICompanyRepository companyRepository,
        ILogger<GetCustomerUseCase> logger,
        IExceptionHandlerService exceptionHandler)
        : base(logger, exceptionHandler)
    {
        _customerRepository = customerRepository;
        _companyRepository = companyRepository;
    }

    /// <summary>
    /// Executa a busca de clientes de um tenant.
    /// </summary>
    /// <param name="phoneNumber">Número de telefone para filtrar (opcional).</param>
    /// <param name="tenantId">ID do tenant.</param>
    /// <returns>Lista de clientes.</returns>
    public async Task<IEnumerable<CustomerResponse>> GetAsync(string? phoneNumber, string tenantId)
    {
        return await ExecuteWithExceptionHandlingAsync(async () =>
        {
            // Validação dos parâmetros de entrada
            ValidateInputParameters(phoneNumber, tenantId);

            // Validação do tenant
            await ValidateTenantAsync(tenantId);

            // Busca dos clientes
            var customers = await GetCustomersAsync(phoneNumber, tenantId);

            // Conversão para DTOs de resposta
            return ConvertToResponseDtos(customers);
        });
    }

    /// <summary>
    /// Valida os parâmetros de entrada.
    /// </summary>
    /// <param name="phoneNumber">Número de telefone.</param>
    /// <param name="tenantId">ID do tenant.</param>
    private void ValidateInputParameters(string? phoneNumber, string tenantId)
    {
        if (string.IsNullOrEmpty(tenantId))
            throw new Hephaestus.Application.Exceptions.ValidationException("ID do tenant é obrigatório.", new ValidationResult());

        if (!string.IsNullOrEmpty(phoneNumber) && phoneNumber.Length < 10)
            throw new Hephaestus.Application.Exceptions.ValidationException("Número de telefone deve ter pelo menos 10 dígitos.", new ValidationResult());
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
            throw new BusinessRuleException("Apenas tenants podem acessar clientes.", "TENANT_ACCESS_RULE");
    }

    /// <summary>
    /// Busca os clientes no repositório.
    /// </summary>
    /// <param name="phoneNumber">Número de telefone para filtrar.</param>
    /// <param name="tenantId">ID do tenant.</param>
    /// <returns>Lista de clientes.</returns>
    private async Task<IEnumerable<Domain.Entities.Customer>> GetCustomersAsync(string? phoneNumber, string tenantId)
    {
        return await _customerRepository.GetAllAsync(phoneNumber, tenantId);
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
            State = c.State,
            City = c.City,
            Street = c.Street,
            Number = c.Number,
            Latitude = c.Latitude,
            Longitude = c.Longitude,
            CreatedAt = c.CreatedAt
        });
    }
}