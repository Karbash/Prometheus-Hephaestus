using System.Security.Claims;

namespace Hephaestus.Application.Interfaces.Category;

public interface IDeleteCategoryUseCase
{
    Task ExecuteAsync(string id, ClaimsPrincipal user);
} 
