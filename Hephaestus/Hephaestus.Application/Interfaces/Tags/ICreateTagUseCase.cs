using Hephaestus.Application.DTOs.Request;
using Hephaestus.Application.DTOs.Response;
using System.Security.Claims;

namespace Hephaestus.Application.Interfaces.Tag;

public interface ICreateTagUseCase
{
    Task<TagResponse> ExecuteAsync(TagRequest request, ClaimsPrincipal user);
}