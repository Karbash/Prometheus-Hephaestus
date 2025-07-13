using Hephaestus.Application.DTOs.Request;

namespace Hephaestus.Application.Interfaces.Additional
{
    public interface IUpdateAdditionalUseCase
    {
        Task ExecuteAsync(string id, UpdateAdditionalRequest request, string tenantId);
    }
}
