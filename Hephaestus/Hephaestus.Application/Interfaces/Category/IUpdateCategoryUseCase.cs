using Hephaestus.Application.DTOs.Request;
using System.Security.Claims;

namespace Hephaestus.Application.Interfaces.Category;

public interface IUpdateCategoryUseCase
{
    Task ExecuteAsync(string id, UpdateCategoryRequest request, ClaimsPrincipal user);
} 