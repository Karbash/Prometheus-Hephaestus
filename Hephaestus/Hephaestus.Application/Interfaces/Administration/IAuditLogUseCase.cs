using Hephaestus.Application.DTOs.Response;
using System.Security.Claims;

namespace Hephaestus.Application.Interfaces.Administration;

public interface IAuditLogUseCase
{
    Task<IEnumerable<AuditLogResponse>> ExecuteAsync(string? adminId, DateTime? startDate, DateTime? endDate, ClaimsPrincipal user);
    Task ExecuteAsync(string action, string entityId, string details, ClaimsPrincipal user);
}
