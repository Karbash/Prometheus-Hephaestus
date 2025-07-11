using System.Security.Claims;

namespace Hephaestus.Application.Interfaces.Tag;

public interface IDeleteTagUseCase
{
    Task ExecuteAsync(string id, ClaimsPrincipal user);
}