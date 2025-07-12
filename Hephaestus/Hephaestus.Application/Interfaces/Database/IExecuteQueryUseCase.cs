using Hephaestus.Application.DTOs.Request;
using Hephaestus.Application.DTOs.Response;

namespace Hephaestus.Application.Interfaces.Database
{
    public interface IExecuteQueryUseCase
    {
        Task<ExecuteQueryResponse> ExecuteAsync(ExecuteQueryRequest request);
    }
}
