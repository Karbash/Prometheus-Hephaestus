using Hephaestus.Application.DTOs.Request;
using Hephaestus.Application.Interfaces.Administration;
using Hephaestus.Domain.Entities;
using Hephaestus.Domain.Enum;
using Hephaestus.Domain.Repositories;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Hephaestus.Application.UseCases.Administration;

public class UpdateCompanyUseCase : IUpdateCompanyUseCase
{
    private readonly ICompanyRepository _companyRepository;

    public UpdateCompanyUseCase(ICompanyRepository companyRepository)
    {
        _companyRepository = companyRepository;
    }

    public async Task ExecuteAsync(string id, CompanyRequest request, ClaimsPrincipal user)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        var company = await _companyRepository.GetByIdAsync(id)
            ?? throw new KeyNotFoundException("Empresa não encontrada.");

        var existingByEmail = await _companyRepository.GetByEmailAsync(request.Email);
        if (existingByEmail != null && existingByEmail.Id != id)
            throw new InvalidOperationException("E-mail já registrado.");

        var existingByPhone = await _companyRepository.GetByPhoneNumberAsync(request.PhoneNumber);
        if (existingByPhone != null && existingByPhone.Id != id)
            throw new InvalidOperationException("Telefone já registrado.");

        var userRole = user?.FindFirst(ClaimTypes.Role)?.Value;
        if (userRole != "Admin")
            throw new InvalidOperationException("Apenas administradores podem atualizar empresas.");

        company.Name = request.Name;
        company.Email = request.Email;
        company.PhoneNumber = request.PhoneNumber;
        company.ApiKey = request.ApiKey;
        company.IsEnabled = request.IsEnabled;
        company.FeeType = Enum.Parse<FeeType>(request.FeeType);
        company.FeeValue = (decimal)request.FeeValue;

        await _companyRepository.UpdateAsync(company);
    }
}