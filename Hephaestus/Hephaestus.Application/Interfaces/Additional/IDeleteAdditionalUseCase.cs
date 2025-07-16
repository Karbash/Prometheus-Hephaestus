using System.Security.Claims;

namespace Hephaestus.Application.Interfaces.Additional
{
    public interface IDeleteAdditionalUseCase
    {
        Task ExecuteAsync(string id, ClaimsPrincipal user);
    }
}
