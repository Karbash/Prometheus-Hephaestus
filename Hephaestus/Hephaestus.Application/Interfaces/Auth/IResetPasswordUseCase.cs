using Hephaestus.Application.DTOs.Request;

namespace Hephaestus.Application.Interfaces.Auth;

public interface IResetPasswordUseCase
{
    Task<string> RequestResetAsync(ResetPasswordRequest request);
    Task ConfirmResetAsync(ResetPasswordConfirmRequest request);
}