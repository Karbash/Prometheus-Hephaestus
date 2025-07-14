using Hephaestus.Application.DTOs.Response;
using System.Security.Claims;

namespace Hephaestus.Application.Interfaces.Administration;

public interface ISalesReportUseCase
{
    Task<SalesReportResponse> ExecuteAsync(DateTime? startDate, DateTime? endDate, ClaimsPrincipal user);
}