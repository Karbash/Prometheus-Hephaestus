using Hephaestus.Domain.DTOs.Request;
using System.Security.Claims;

namespace Hephaestus.Application.Interfaces.Category;

public interface ICreateCategoryUseCase
{
    Task<string> ExecuteAsync(CreateCategoryRequest request, ClaimsPrincipal user);
} 
