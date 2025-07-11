using Hephaestus.Application.DTOs.Request;
using Hephaestus.Application.DTOs.Response;

namespace Hephaestus.Application.Interfaces.OpenAI
{
    public interface IChatWithOpenAIUseCase
    {
        Task<OpenAIChatResponse> ExecuteAsync(OpenAIChatRequest request);
    }
}
