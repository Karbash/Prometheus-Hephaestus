using Hephaestus.Application.DTOs.Response;

namespace Hephaestus.Application.Interfaces.Additional
{
    public interface IGetAdditionalByIdUseCase
    {
        Task<AdditionalResponse> ExecuteAsync(string id, string tenantId);
    }
}
