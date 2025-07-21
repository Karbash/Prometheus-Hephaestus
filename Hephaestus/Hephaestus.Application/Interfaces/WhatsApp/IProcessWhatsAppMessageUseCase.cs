using Hephaestus.Domain.DTOs.Request;
using Hephaestus.Domain.DTOs.Response;

namespace Hephaestus.Application.Interfaces.WhatsApp;

public interface IProcessWhatsAppMessageUseCase
{
    Task<WhatsAppResponse> ExecuteAsync(WhatsAppMessageRequest request);
} 