using Hephaestus.Application.DTOs.Request;
using Hephaestus.Application.Interfaces.Administration;
using Hephaestus.Domain.Entities;
using Hephaestus.Domain.Repositories;
using System.Security.Claims;

namespace Hephaestus.Application.UseCases.Administration;

/// <summary>
/// Caso de uso para atualização de empresas existentes.
/// </summary>
public class UpdateCompanyUseCase : IUpdateCompanyUseCase
{
    private readonly ICompanyRepository _companyRepository;
    private readonly IAuditLogRepository _auditLogRepository;

    /// <summary>
    /// Inicializa uma nova instância do <see cref="UpdateCompanyUseCase"/>.
    /// </summary>
    /// <param name="companyRepository">Repositório de empresas.</param>
    /// <param name="auditLogRepository">Repositório de logs de auditoria.</param>
    public UpdateCompanyUseCase(ICompanyRepository companyRepository, IAuditLogRepository auditLogRepository)
    {
        _companyRepository = companyRepository;
        _auditLogRepository = auditLogRepository;
    }

    /// <summary>
    /// Atualiza os dados de uma empresa existente e registra um log de auditoria.
    /// </summary>
    /// <param name="id">Identificador da empresa a ser atualizada.</param>
    /// <param name="request">Dados da empresa para atualização.</param>
    /// <param name="user">Claims do usuário autenticado.</param>
    /// <returns>Task representando a operação assíncrona.</returns>
    /// <exception cref="ArgumentNullException">Se request for nulo.</exception>
    /// <exception cref="KeyNotFoundException">Se a empresa não for encontrada.</exception>
    /// <exception cref="InvalidOperationException">Se e-mail ou telefone já estiver registrado ou usuário não for administrador.</exception>
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

        // Atualizar apenas os campos fornecidos (suporte a atualização parcial)
        company.Name = request.Name ?? company.Name;
        company.Email = request.Email ?? company.Email;
        company.PhoneNumber = request.PhoneNumber ?? company.PhoneNumber;
        company.ApiKey = request.ApiKey ?? company.ApiKey;
        company.IsEnabled = request.IsEnabled ?? company.IsEnabled;
        company.FeeType = request.FeeType ?? company.FeeType;
        company.FeeValue = request.FeeValue ?? company.FeeValue;
        company.City = request.City ?? company.City;
        company.Street = request.Street ?? company.Street;
        company.Number = request.Number ?? company.Number;
        company.Latitude = request.Latitude ?? company.Latitude;
        company.Longitude = request.Longitude ?? company.Longitude;
        company.Slogan = request.Slogan ?? company.Slogan;
        company.Description = request.Description ?? company.Description;

        await _companyRepository.UpdateAsync(company);

        // Registrar log de auditoria
        var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? throw new InvalidOperationException("Usuário não autenticado.");

        await _auditLogRepository.AddAsync(new AuditLog
        {
            UserId = userId,
            Action = "Atualização de Empresa",
            EntityId = company.Id,
            Details = $"Empresa {company.Name} atualizada com e-mail {company.Email}.",
            CreatedAt = DateTime.UtcNow
        });
    }
}