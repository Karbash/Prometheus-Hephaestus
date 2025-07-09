using Hephaestus.Application.DTOs.Request;

namespace Hephaestus.Application.Interfaces
{
    public interface IRegisterCompanyUseCase
    {
        Task<string> ExecuteAsync(RegisterCompanyRequest request);
    }
}
