using Hephaestus.Domain.DTOs.Response;
using System.Security.Claims;

namespace Hephaestus.Application.Interfaces.Additional
{
    public interface IGetAdditionalByIdUseCase
    {
        Task<AdditionalResponse> ExecuteAsync(string id, ClaimsPrincipal user);
    }
}
