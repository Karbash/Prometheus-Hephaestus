using Hephaestus.Application.DTOs.Response;

namespace Hephaestus.Application.Interfaces.Additional
{
    public interface IGetAdditionalsUseCase
    {
        Task<IEnumerable<AdditionalResponse>> ExecuteAsync(string tenantId);
    }
}
