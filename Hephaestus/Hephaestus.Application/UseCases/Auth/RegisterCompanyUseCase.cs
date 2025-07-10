using Hephaestus.Application.DTOs.Request;
using Hephaestus.Application.Interfaces.Auth;
using Hephaestus.Domain.Entities;
using Hephaestus.Domain.Enum;
using Hephaestus.Domain.Repositories;
using Hephaestus.Domain.Services;
using System.Security.Claims;

namespace Hephaestus.Application.UseCases.Auth;

/// <summary>
/// Caso de uso para registro de novas empresas.
/// </summary>
public class RegisterCompanyUseCase : IRegisterCompanyUseCase
{
    private readonly ICompanyRepository _companyRepository;
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly ILoggedUserService _loggedUserService;

    /// <summary>
    /// Inicializa uma nova instância do <see cref="RegisterCompanyUseCase"/>.
    /// </summary>
    /// <param name="companyRepository">Repositório de empresas.</param>
    /// <param name="auditLogRepository">Repositório de logs de auditoria.</param>
    /// <param name="loggedUserService">Serviço para obter informações do usuário logado.</param>
    public RegisterCompanyUseCase(
        ICompanyRepository companyRepository,
        IAuditLogRepository auditLogRepository,
        ILoggedUserService loggedUserService)
    {
        _companyRepository = companyRepository;
        _auditLogRepository = auditLogRepository;
        _loggedUserService = loggedUserService;
    }

    /// <summary>
    /// Registra uma nova empresa e adiciona um log de auditoria.
    /// </summary>
    /// <param name="request">Dados da empresa a ser registrada.</param>
    /// <param name="claimsPrincipal">Claims do usuário autenticado, ou null se não autenticado.</param>
    /// <returns>ID da empresa registrada.</returns>
    /// <exception cref="ArgumentNullException">Se request for nulo.</exception>
    /// <exception cref="InvalidOperationException">E-mail ou telefone já registrado, ou usuário não autorizado.</exception>
    public async Task<string> ExecuteAsync(RegisterCompanyRequest request, ClaimsPrincipal? claimsPrincipal)
    {
        // Validate input
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        // Check if email or phone already exists
        var existingByEmail = await _companyRepository.GetByEmailAsync(request.Email);
        if (existingByEmail != null)
            throw new InvalidOperationException("E-mail já registrado.");

        var existingByPhone = await _companyRepository.GetByPhoneNumberAsync(request.PhoneNumber);
        if (existingByPhone != null)
            throw new InvalidOperationException("Telefone já registrado.");

        // Ensure only admins can register companies
        var userRole = claimsPrincipal?.FindFirst(ClaimTypes.Role)?.Value;
        if (claimsPrincipal == null || userRole != "Admin")
            throw new InvalidOperationException("Apenas administradores podem registrar empresas.");

        // Create company entity
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

        // Save to database
        await _companyRepository.AddAsync(company);

        // Log audit
        var adminId = claimsPrincipal != null
            ? (await _loggedUserService.GetLoggedUserAsync(claimsPrincipal)).Id
            : throw new InvalidOperationException("Usuário não autenticado.");

        await _auditLogRepository.AddAsync(new AuditLog
        {
            UserId = adminId,
            Action = "Registro de Empresa",
            EntityId = company.Id,
            Details = $"Empresa {company.Name} registrada com e-mail {company.Email}.",
            CreatedAt = DateTime.UtcNow
        });

        return company.Id;
    }
}