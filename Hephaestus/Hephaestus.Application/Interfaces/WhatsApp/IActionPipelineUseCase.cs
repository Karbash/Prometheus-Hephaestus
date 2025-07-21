using Hephaestus.Domain.DTOs.Request;
using Hephaestus.Domain.DTOs.Response;

namespace Hephaestus.Application.Interfaces.WhatsApp;

public interface IActionPipelineUseCase
{
    Task<WhatsAppResponse> ExecutePipelineAsync(List<int> codes, Dictionary<string, object>? data, string phoneNumber);
} 