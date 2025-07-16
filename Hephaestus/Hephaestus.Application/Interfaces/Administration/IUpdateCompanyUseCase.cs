using Hephaestus.Domain.DTOs.Request;
using System.Security.Claims;

namespace Hephaestus.Application.Interfaces.Administration;

public interface IUpdateCompanyUseCase
{
    Task ExecuteAsync(string id, UpdateCompanyRequest request, ClaimsPrincipal user);
}
