using Hephaestus.Domain.DTOs.Request;
using Hephaestus.Domain.DTOs.Response;

namespace Hephaestus.Application.Interfaces.Database
{
    public interface IExecuteQueryUseCase
    {
        Task<ExecuteQueryResponse> ExecuteAsync(ExecuteQueryRequest request);
    }
}
