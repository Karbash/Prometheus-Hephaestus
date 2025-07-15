using Hephaestus.Domain.DTOs.Response;
using System.Security.Claims;
using Hephaestus.Domain.DTOs.Request;

namespace Hephaestus.Application.Interfaces.Tag;

public interface ICreateTagUseCase
{
    Task<TagResponse> ExecuteAsync(TagRequest request, ClaimsPrincipal user);
}