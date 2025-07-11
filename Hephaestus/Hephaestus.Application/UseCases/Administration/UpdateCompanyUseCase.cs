using Hephaestus.Application.DTOs.Request;
using Hephaestus.Application.Interfaces.Administration;
using Hephaestus.Domain.Entities;
using Hephaestus.Domain.Repositories;
using System.Security.Claims;

namespace Hephaestus.Application.UseCases.Administration;

public class UpdateCompanyUseCase : IUpdateCompanyUseCase
{
    private readonly ICompanyRepository _companyRepository;
    private readonly IAuditLogRepository _auditLogRepository;

    public UpdateCompanyUseCase(ICompanyRepository companyRepository, IAuditLogRepository auditLogRepository)
    {
        _companyRepository = companyRepository;
        _auditLogRepository = auditLogRepository;
    }

    public async Task ExecuteAsync(string id, UpdateCompanyRequest request, ClaimsPrincipal user)
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

        company.Name = request.Name ?? company.Name;
        company.Email = request.Email ?? company.Email;
        company.PhoneNumber = request.PhoneNumber ?? company.PhoneNumber;
        company.ApiKey = request.ApiKey ?? company.ApiKey;
        company.IsEnabled = request.IsEnabled ?? company.IsEnabled;
        company.FeeType = request.FeeType ?? company.FeeType;
        company.FeeValue = request.FeeValue ?? company.FeeValue;
        company.State = request.State ?? company.State; // Novo campo
        company.City = request.City ?? company.City;
        company.Street = request.Street ?? company.Street;
        company.Number = request.Number ?? company.Number;
        company.Latitude = request.Latitude ?? company.Latitude;
        company.Longitude = request.Longitude ?? company.Longitude;
        company.Slogan = request.Slogan ?? company.Slogan;
        company.Description = request.Description ?? company.Description;

        await _companyRepository.UpdateAsync(company);

        var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? throw new InvalidOperationException("Usuário não autenticado.");

        await _auditLogRepository.AddAsync(new AuditLog
        {
            UserId = userId,
            Action = "Atualização de Empresa",
            EntityId = company.Id,
            Details = $"Empresa {company.Name} atualizada com e-mail {company.Email} no estado {company.State}.",
            CreatedAt = DateTime.UtcNow
        });
    }
}