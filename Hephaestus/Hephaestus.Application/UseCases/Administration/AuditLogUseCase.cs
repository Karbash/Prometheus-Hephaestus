using Hephaestus.Application.DTOs.Response;
using Hephaestus.Application.Interfaces.Administration;
using Hephaestus.Domain.Entities;
using Hephaestus.Domain.Repositories;
using Hephaestus.Domain.Services;
using System.Security.Claims;

namespace Hephaestus.Application.UseCases.Administration;

public class AuditLogUseCase : IAuditLogUseCase
{
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly ILoggedUserService _loggedUserService;
    private readonly ICompanyRepository _companyRepository;

    public AuditLogUseCase(IAuditLogRepository auditLogRepository, ILoggedUserService loggedUserService, ICompanyRepository companyRepository)
    {
        _auditLogRepository = auditLogRepository;
        _loggedUserService = loggedUserService;
        _companyRepository = companyRepository;
    }

    public async Task<IEnumerable<AuditLogResponse>> ExecuteAsync(string? adminId, DateTime? startDate, DateTime? endDate, ClaimsPrincipal user)
    {
        var userRole = user?.FindFirst(ClaimTypes.Role)?.Value;
        if (userRole != "Admin")
            throw new InvalidOperationException("Apenas administradores podem acessar logs de auditoria.");

        // Validar adminId, se fornecido
        if (!string.IsNullOrEmpty(adminId))
        {
            var admin = await _companyRepository.GetByIdAsync(adminId);
            if (admin == null || admin.Role.ToString() != "Admin")
                throw new InvalidOperationException("Admin inválido.");
        }

        // Validar datas
        if (startDate.HasValue && endDate.HasValue && startDate > endDate)
            throw new InvalidOperationException("A data inicial não pode ser posterior à data final.");

        // Normalizar datas para UTC
        var normalizedStartDate = startDate?.ToUniversalTime();
        var normalizedEndDate = endDate?.ToUniversalTime();

        var logs = await _auditLogRepository.GetLogsAsync(adminId, normalizedStartDate, normalizedEndDate);
        return logs.Select(l => new AuditLogResponse
        {
            Id = l.Id,
            AdminId = l.AdminId,
            Action = l.Action,
            EntityId = l.EntityId,
            Details = l.Details,
            CreatedAt = l.CreatedAt
        });
    }

    public async Task ExecuteAsync(string action, string entityId, string details, ClaimsPrincipal user)
    {
        var userRole = user?.FindFirst(ClaimTypes.Role)?.Value;
        if (userRole != "Admin")
            throw new InvalidOperationException("Apenas administradores podem registrar logs de auditoria.");

        var admin = await _loggedUserService.GetLoggedUserAsync(user);
        await _auditLogRepository.AddAsync(new AuditLog
        {
            AdminId = admin.Id,
            Action = action,
            EntityId = entityId,
            Details = details,
            CreatedAt = DateTime.UtcNow
        });
    }
}