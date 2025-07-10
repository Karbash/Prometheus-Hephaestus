using Hephaestus.Application.DTOs.Request;

namespace Hephaestus.Application.Interfaces.Auth;

public interface IMfaUseCase
{
    Task<string> ExecuteAsync(MfaRequest request);
    Task<string> SetupMfaAsync(string email);
}