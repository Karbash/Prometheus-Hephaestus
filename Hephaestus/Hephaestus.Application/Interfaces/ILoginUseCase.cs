using Hephaestus.Application.DTOs.Request;

namespace Hephaestus.Application.Interfaces;

public interface ILoginUseCase
{
    Task<string> ExecuteAsync(LoginRequest request);
}