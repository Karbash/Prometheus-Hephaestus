using Hephaestus.Application.DTOs.Response;
using Hephaestus.Application.Interfaces.Administration;
using Hephaestus.Domain.Entities;
using Hephaestus.Domain.Repositories;
using Hephaestus.Domain.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

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

    public async Task<IEnumerable<AuditLogResponse>> ExecuteAsync(string? userId, DateTime? startDate, DateTime? endDate, ClaimsPrincipal user)
    {
        var userRole = user?.FindFirst(ClaimTypes.Role)?.Value;
        if (userRole != "Admin")
            throw new InvalidOperationException("Apenas administradores podem acessar logs de auditoria.");

        // Validar userId, se fornecido
        if (!string.IsNullOrEmpty(userId))
        {
            var company = await _companyRepository.GetByIdAsync(userId);
            if (company == null)
                throw new InvalidOperationException("Usuário inválido.");
        }

        // Validar datas
        if (startDate.HasValue && endDate.HasValue && startDate > endDate)
            throw new InvalidOperationException("A data inicial não pode ser posterior à data final.");

        // Normalizar datas para UTC
        var normalizedStartDate = startDate?.ToUniversalTime();
        var normalizedEndDate = endDate?.ToUniversalTime();

        var logs = await _auditLogRepository.GetLogsAsync(userId, normalizedStartDate, normalizedEndDate);
        return logs.Select(l => new AuditLogResponse
        {
            Id = l.Id,
            UserId = l.UserId, // Alterado de AdminId para UserId
            Action = l.Action,
            EntityId = l.EntityId,
            Details = l.Details,
            CreatedAt = l.CreatedAt
        });
    }

    public async Task ExecuteAsync(string action, string entityId, string details, ClaimsPrincipal user)
    {
        var userRole = user?.FindFirst(ClaimTypes.Role)?.Value;
        if (userRole != "Admin" && userRole != "Tenant")
            throw new InvalidOperationException("Apenas administradores e tenants podem registrar logs de auditoria.");

        var userId = user?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "Unknown";
        var tenantId = userRole == "Tenant" ? user?.FindFirst("TenantId")?.Value ?? string.Empty : null;

        var auditLog = new AuditLog
        {
            Id = Guid.NewGuid().ToString(),
            UserId = userId, // Preencher UserId para Admin e Tenant
            TenantId = tenantId, // TenantId apenas para Tenant
            Action = action,
            EntityId = entityId,
            Details = details,
            CreatedAt = DateTime.UtcNow
        };

        await _auditLogRepository.AddAsync(auditLog);
    }
}