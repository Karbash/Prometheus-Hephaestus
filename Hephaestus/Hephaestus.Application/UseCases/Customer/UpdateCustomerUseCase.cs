using Hephaestus.Application.DTOs.Request;
using Hephaestus.Application.Interfaces.Customer;
using Hephaestus.Domain.Entities;
using Hephaestus.Domain.Repositories;

namespace Hephaestus.Application.UseCases.Customer;

public class UpdateCustomerUseCase : IUpdateCustomerUseCase
{
    private readonly ICustomerRepository _customerRepository;
    private readonly ICompanyRepository _companyRepository;

    public UpdateCustomerUseCase(ICustomerRepository customerRepository, ICompanyRepository companyRepository)
    {
        _customerRepository = customerRepository;
        _companyRepository = companyRepository;
    }

    public async Task UpdateAsync(CustomerRequest request, string tenantId)
    {
        if (string.IsNullOrEmpty(request.PhoneNumber))
            throw new InvalidOperationException("O número de telefone é obrigatório.");
        if (string.IsNullOrEmpty(request.State))
            throw new InvalidOperationException("O estado é obrigatório.");

        var company = await _companyRepository.GetByIdAsync(tenantId);
        if (company == null || company.Role.ToString() != "Tenant")
            throw new InvalidOperationException("Tenant inválido.");

        var existingCustomer = await _customerRepository.GetByPhoneNumberAsync(request.PhoneNumber, tenantId);

        var customer = new Domain.Entities.Customer
        {
            Id = existingCustomer?.Id ?? Guid.NewGuid().ToString(),
            TenantId = tenantId,
            PhoneNumber = request.PhoneNumber,
            Name = request.Name,
            State = request.State, // Novo campo
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