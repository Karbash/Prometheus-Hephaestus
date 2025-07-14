using Hephaestus.Application.DTOs.Request;
using System.Security.Claims;

namespace Hephaestus.Application.Interfaces.Additional
{
    public interface IUpdateAdditionalUseCase
    {
        Task ExecuteAsync(string id, UpdateAdditionalRequest request, ClaimsPrincipal user);
    }
}
