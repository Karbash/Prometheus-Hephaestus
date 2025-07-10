using Hephaestus.Application.DTOs.Request;
using System.Security.Claims;

namespace Hephaestus.Application.Interfaces.Administration;

public interface IUpdateCompanyUseCase
{
    Task ExecuteAsync(string id, CompanyRequest request, ClaimsPrincipal user);
}
