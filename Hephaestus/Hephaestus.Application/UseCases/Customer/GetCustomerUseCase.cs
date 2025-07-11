using Hephaestus.Application.DTOs.Response;
using Hephaestus.Application.Interfaces.Customer;
using Hephaestus.Domain.Repositories;

namespace Hephaestus.Application.UseCases.Customer;

public class GetCustomerUseCase : IGetCustomerUseCase
{
    private readonly ICustomerRepository _customerRepository;
    private readonly ICompanyRepository _companyRepository;

    public GetCustomerUseCase(ICustomerRepository customerRepository, ICompanyRepository companyRepository)
    {
        _customerRepository = customerRepository;
        _companyRepository = companyRepository;
    }

    public async Task<IEnumerable<CustomerResponse>> GetAsync(string? phoneNumber, string tenantId)
    {
        var company = await _companyRepository.GetByIdAsync(tenantId);
        if (company == null || company.Role.ToString() != "Tenant")
            throw new InvalidOperationException("Tenant inválido.");

        var customers = await _customerRepository.GetAllAsync(phoneNumber, tenantId);
        return customers.Select(c => new CustomerResponse
        {
            Id = c.Id,
            TenantId = c.TenantId,
            PhoneNumber = c.PhoneNumber,
            Name = c.Name,
            State = c.State, // Novo campo
            City = c.City,
            Street = c.Street,
            Number = c.Number,
            Latitude = c.Latitude,
            Longitude = c.Longitude,
            CreatedAt = c.CreatedAt
        });
    }
}