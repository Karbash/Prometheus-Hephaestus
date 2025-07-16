using Hephaestus.Domain.DTOs.Request;
using System.Security.Claims;

namespace Hephaestus.Application.Interfaces.Additional
{
    public interface ICreateAdditionalUseCase
    {
        Task<string> ExecuteAsync(CreateAdditionalRequest request, ClaimsPrincipal user);
    }
}
