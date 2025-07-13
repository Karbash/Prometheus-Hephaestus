using Hephaestus.Application.DTOs.Request;

namespace Hephaestus.Application.Interfaces.Additional
{
    public interface ICreateAdditionalUseCase
    {
        Task<string> ExecuteAsync(CreateAdditionalRequest request, string tenantId);
    }
}
