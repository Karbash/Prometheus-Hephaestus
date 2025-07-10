using Hephaestus.Application.DTOs.Response;
using Hephaestus.Application.Interfaces.Customer;
using Hephaestus.Domain.Repositories;

namespace Hephaestus.Application.UseCases.Customer;

public class GetByIdCustomerUseCase : IGetByIdCustomerUseCase
{
    private readonly ICustomerRepository _customerRepository;
    private readonly ICompanyRepository _companyRepository;

    public GetByIdCustomerUseCase(ICustomerRepository customerRepository, ICompanyRepository companyRepository)
    {
        _customerRepository = customerRepository;
        _companyRepository = companyRepository;
    }

    public async Task<CustomerResponse?> GetByIdAsync(string id, string tenantId)
    {
        var company = await _companyRepository.GetByIdAsync(tenantId);
        if (company == null || company.Role.ToString() != "Tenant")
            throw new InvalidOperationException("Tenant inválido.");

        var customer = await _customerRepository.GetByIdAsync(id, tenantId);
        if (customer == null)
            return null;

        return new CustomerResponse
        {
            Id = customer.Id,
            TenantId = customer.TenantId,
            PhoneNumber = customer.PhoneNumber,
            Name = customer.Name,
            Address = customer.Address,
            Latitude = customer.Latitude,
            Longitude = customer.Longitude,
            CreatedAt = customer.CreatedAt
        };
    }
}