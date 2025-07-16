using Hephaestus.Domain.DTOs.Response;
using System.Security.Claims;

namespace Hephaestus.Application.Interfaces.Additional
{
    public interface IGetAdditionalsUseCase
    {
        Task<PagedResult<AdditionalResponse>> ExecuteAsync(System.Security.Claims.ClaimsPrincipal user, int pageNumber = 1, int pageSize = 20, string? sortBy = null, string? sortOrder = "asc");
    }
}
