using Hephaestus.Domain.DTOs.Request;

namespace Hephaestus.Application.Interfaces.OpenAI
{
    public interface IChatWithOpenAIUseCase
    {
        Task<OpenAIResponse> ExecuteAsync(OpenAIRequest request);
    }
}
