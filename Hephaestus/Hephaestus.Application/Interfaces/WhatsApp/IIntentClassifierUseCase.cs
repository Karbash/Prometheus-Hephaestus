using Hephaestus.Domain.DTOs.Request;
using Hephaestus.Domain.DTOs.Response;

namespace Hephaestus.Application.Interfaces.WhatsApp;

public interface IIntentClassifierUseCase
{
    Task<WhatsAppResponse> ClassifyIntentAsync(string message, string? conversationContext = null);
} 