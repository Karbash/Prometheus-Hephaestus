using Hephaestus.Domain.DTOs.Request;

namespace Hephaestus.Application.Interfaces.Auth;

public interface ILoginUseCase
{
    Task<string> ExecuteAsync(LoginRequest request, string? mfaCode = null);
}
