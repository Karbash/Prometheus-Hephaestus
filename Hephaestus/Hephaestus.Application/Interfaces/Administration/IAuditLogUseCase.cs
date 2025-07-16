using Hephaestus.Domain.DTOs.Response;
using System.Security.Claims;

namespace Hephaestus.Application.Interfaces.Administration;

public interface IAuditLogUseCase
{
    Task<IEnumerable<AuditLogResponse>> ExecuteAsync(DateTime? startDate, DateTime? endDate, ClaimsPrincipal user);
    Task ExecuteAsync(string action, string entityId, string details, ClaimsPrincipal user);
}
