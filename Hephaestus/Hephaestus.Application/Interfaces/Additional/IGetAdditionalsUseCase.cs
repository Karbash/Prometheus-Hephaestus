using Hephaestus.Application.DTOs.Response;
using System.Security.Claims;

namespace Hephaestus.Application.Interfaces.Additional
{
    public interface IGetAdditionalsUseCase
    {
        Task<IEnumerable<AdditionalResponse>> ExecuteAsync(ClaimsPrincipal user);
    }
}
