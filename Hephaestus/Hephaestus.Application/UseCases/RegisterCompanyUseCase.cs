using Hephaestus.Application.DTOs.Request;
using Hephaestus.Application.Interfaces;
using Hephaestus.Domain.Entities;
using Hephaestus.Domain.Repositories;

namespace Hephaestus.Application.UseCases;

public class RegisterCompanyUseCase : IRegisterCompanyUseCase
{
    private readonly ICompanyRepository _companyRepository;

    public RegisterCompanyUseCase(ICompanyRepository companyRepository)
    {
        _companyRepository = companyRepository;
    }

    public async Task<string> ExecuteAsync(RegisterCompanyRequest request)
    {
        var existingByEmail = await _companyRepository.GetByEmailAsync(request.Email);
        if (existingByEmail != null)
            throw new InvalidOperationException("E-mail já registrado.");

        var existingByPhone = await _companyRepository.GetByPhoneNumberAsync(request.PhoneNumber);
        if (existingByPhone != null)
            throw new InvalidOperationException("Telefone já registrado.");

        var company = new Company
        {
            Id = Guid.NewGuid().ToString(),
            Name = request.Name,
            Email = request.Email,
            PhoneNumber = request.PhoneNumber,
            ApiKey = request.ApiKey,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Role = Role.Tenant,
            IsEnabled = request.IsEnabled,
            FeeType = request.FeeType,
            FeeValue = request.FeeValue
        };

        await _companyRepository.AddAsync(company);
        return company.Id;
    }
}