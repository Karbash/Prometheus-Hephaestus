using Hephaestus.Application.DTOs.Response;
using System.Security.Claims;

namespace Hephaestus.Application.Interfaces.Tag;

public interface IGetAllTagsByTenantUseCase
{
    Task<IEnumerable<TagResponse>> ExecuteAsync(string tenantId, ClaimsPrincipal user);
}