using Hephaestus.Domain.DTOs.Response;
using System.Security.Claims;

namespace Hephaestus.Application.Interfaces.Category;

public interface IGetCategoryByIdUseCase
{
    Task<CategoryResponse?> ExecuteAsync(string id, ClaimsPrincipal user);
} 