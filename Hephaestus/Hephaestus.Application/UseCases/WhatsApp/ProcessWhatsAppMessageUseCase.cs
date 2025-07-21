using Hephaestus.Application.Base;
using Hephaestus.Application.Interfaces.WhatsApp;
using Hephaestus.Application.Services;
using Hephaestus.Domain.DTOs.Request;
using Hephaestus.Domain.DTOs.Response;
using Hephaestus.Domain.Entities;
using Hephaestus.Domain.Repositories;
using Microsoft.Extensions.Logging;
using FluentValidation.Results;

namespace Hephaestus.Application.UseCases.WhatsApp;

/// <summary>
/// Caso de uso principal para processamento de mensagens do WhatsApp
/// </summary>
public class ProcessWhatsAppMessageUseCase : BaseUseCase, IProcessWhatsAppMessageUseCase
{
    private readonly IIntentClassifierUseCase _intentClassifier;
    private readonly IActionPipelineUseCase _actionPipeline;
    private readonly IConversationContextService _conversationContextService;

    public ProcessWhatsAppMessageUseCase(
        IIntentClassifierUseCase intentClassifier,
        IActionPipelineUseCase actionPipeline,
        IConversationContextService conversationContextService,
        ILogger<ProcessWhatsAppMessageUseCase> logger,
        IExceptionHandlerService exceptionHandler)
        : base(logger, exceptionHandler)
    {
        _intentClassifier = intentClassifier;
        _actionPipeline = actionPipeline;
        _conversationContextService = conversationContextService;
    }

    public async Task<WhatsAppResponse> ExecuteAsync(WhatsAppMessageRequest request)
    {
        return await ExecuteWithExceptionHandlingAsync(async () =>
        {
            // Validação da requisição
            ValidateRequest(request);

            // Processa baseado no tipo de mensagem
            WhatsAppResponse response;

            // Verifica se há dados de localização no contexto
            bool hasLocationData = request.ContextData != null && 
                                 request.ContextData.ContainsKey("latitude") && 
                                 request.ContextData.ContainsKey("longitude");

            if (request.Type == "location" && request.Latitude.HasValue && request.Longitude.HasValue)
            {
                // Usuário enviou localização
                response = await ProcessLocationMessageAsync(request);
            }
            else if (request.Type == "text" && !string.IsNullOrEmpty(request.Text))
            {
                // Usuário enviou texto
                if (hasLocationData)
                {
                    // Se há dados de localização no contexto, processa como localização
                    response = await ProcessTextWithLocationAsync(request);
                }
                else
                {
                    // Processa como texto normal
                    response = await ProcessTextMessageAsync(request);
                }
            }
            else
            {
                // Tipo de mensagem não suportado
                response = new WhatsAppResponse
                {
                    Message = "Desculpe, não consigo processar este tipo de mensagem. Por favor, envie um texto ou sua localização.",
                    WaitForResponse = true
                };
            }

            return response;
        }, "ProcessWhatsAppMessage");
    }

    private void ValidateRequest(WhatsAppMessageRequest request)
    {
        if (request == null)
            throw new Application.Exceptions.ValidationException("Dados da mensagem são obrigatórios.", new ValidationResult());

        // Validação mais robusta do número de telefone
        if (string.IsNullOrWhiteSpace(request.From))
        {
            // Se não tem From, tenta usar ConversationId como fallback
            if (!string.IsNullOrWhiteSpace(request.ConversationId))
            {
                // Extrai número do ConversationId se for no formato session_XXXXXXXX
                var sessionId = request.ConversationId;
                if (sessionId.StartsWith("session_"))
                {
                    var phoneNumber = sessionId.Replace("session_", "");
                    if (!string.IsNullOrWhiteSpace(phoneNumber))
                    {
                        request.From = phoneNumber;
                    }
                }
            }
            
            // Se ainda não tem From, lança exceção
            if (string.IsNullOrWhiteSpace(request.From))
            {
                throw new Application.Exceptions.ValidationException("Número de telefone do usuário é obrigatório.", new ValidationResult());
            }
        }

        if (string.IsNullOrEmpty(request.Type))
            throw new Application.Exceptions.ValidationException("Tipo da mensagem é obrigatório.", new ValidationResult());
    }



    private async Task<WhatsAppResponse> ProcessLocationMessageAsync(WhatsAppMessageRequest request)
    {
        // Gera um sessionId baseado no número de telefone
        var sessionId = $"session_{request.From}";
        
        // Obtém ou cria sessão de conversa
        var session = await _conversationContextService.GetOrCreateSessionAsync(sessionId, request.From);
        
        // Verifica se pode pular a OpenAI
        var (canSkipOpenAI, intent, response) = await _conversationContextService.CanSkipOpenAIAsync("Usuário enviou localização", session);
        
        if (canSkipOpenAI)
        {
            // Resposta sem usar OpenAI
            var quickResponse = new WhatsAppResponse
            {
                Message = response!,
                WaitForResponse = true
            };
            
            // Atualiza contexto da sessão primeiro
            await _conversationContextService.UpdateSessionContextAsync(sessionId, intent);
            
            // Registra a mensagem sem OpenAI
            await _conversationContextService.AddMessageAsync(sessionId, "Usuário enviou localização", intent, response, false);
            
            return quickResponse;
        }
        
        // Classifica a intenção usando OpenAI
        var classification = await _intentClassifier.ClassifyIntentAsync("Usuário enviou localização", session.LastIntent);
        
        // Registra a mensagem com OpenAI
        await _conversationContextService.AddMessageAsync(sessionId, "Usuário enviou localização", classification.Codes.FirstOrDefault().ToString(), classification.Message, true);
        
        // Atualiza contexto da sessão
        await _conversationContextService.UpdateSessionContextAsync(sessionId, classification.Codes.FirstOrDefault().ToString());

        // Se há códigos para executar, executa o pipeline
        if (classification.Codes.Any())
        {
            var data = new Dictionary<string, object>
            {
                ["latitude"] = request.Latitude!.Value,
                ["longitude"] = request.Longitude!.Value,
                ["phone_number"] = request.From
            };

            return await _actionPipeline.ExecutePipelineAsync(classification.Codes, data, request.From);
        }

        // Se não há códigos, retorna a resposta direta da classificação
        return classification;
    }

    private async Task<WhatsAppResponse> ProcessTextWithLocationAsync(WhatsAppMessageRequest request)
    {
        // Gera um sessionId baseado no número de telefone
        var sessionId = $"session_{request.From}";
        
        // Obtém ou cria sessão de conversa
        var session = await _conversationContextService.GetOrCreateSessionAsync(sessionId, request.From);
        
        // Extrai dados de localização do contexto
        var latitude = ExtractDoubleValue(request.ContextData!["latitude"]);
        var longitude = ExtractDoubleValue(request.ContextData!["longitude"]);
        
        // Verifica se pode pular a OpenAI
        var (canSkipOpenAI, intent, response) = await _conversationContextService.CanSkipOpenAIAsync(request.Text!, session);
        
        if (canSkipOpenAI)
        {
            // Resposta sem usar OpenAI
            var quickResponse = new WhatsAppResponse
            {
                Message = response!,
                WaitForResponse = true
            };
            
            // Atualiza contexto da sessão primeiro
            await _conversationContextService.UpdateSessionContextAsync(sessionId, intent);
            
            // Registra a mensagem sem OpenAI
            await _conversationContextService.AddMessageAsync(sessionId, request.Text!, intent, response, false);
            
            return quickResponse;
        }
        
        // Classifica a intenção usando OpenAI
        var classification = await _intentClassifier.ClassifyIntentAsync(request.Text!, session.LastIntent);
        
        // Registra a mensagem com OpenAI
        await _conversationContextService.AddMessageAsync(sessionId, request.Text!, classification.Codes.FirstOrDefault().ToString(), classification.Message, true);
        
        // Atualiza contexto da sessão
        await _conversationContextService.UpdateSessionContextAsync(sessionId, classification.Codes.FirstOrDefault().ToString());

        // Se há códigos para executar, executa o pipeline
        if (classification.Codes.Any())
        {
            var data = new Dictionary<string, object>
            {
                ["latitude"] = latitude,
                ["longitude"] = longitude,
                ["phone_number"] = request.From,
                ["message"] = request.Text!
            };

            return await _actionPipeline.ExecutePipelineAsync(classification.Codes, data, request.From);
        }

        // Se não há códigos, retorna a resposta direta da classificação
        return classification;
    }

    private async Task<WhatsAppResponse> ProcessTextMessageAsync(WhatsAppMessageRequest request)
    {
        // Gera um sessionId baseado no número de telefone
        var sessionId = $"session_{request.From}";
        
        // Obtém ou cria sessão de conversa
        var session = await _conversationContextService.GetOrCreateSessionAsync(sessionId, request.From);
        
        // Verifica se pode pular a OpenAI
        var (canSkipOpenAI, intent, response) = await _conversationContextService.CanSkipOpenAIAsync(request.Text!, session);
        
        if (canSkipOpenAI)
        {
            // Resposta sem usar OpenAI
            var quickResponse = new WhatsAppResponse
            {
                Message = response!,
                WaitForResponse = true
            };
            
            // Atualiza contexto da sessão primeiro
            await _conversationContextService.UpdateSessionContextAsync(sessionId, intent);
            
            // Registra a mensagem sem OpenAI
            await _conversationContextService.AddMessageAsync(sessionId, request.Text!, intent, response, false);
            
            return quickResponse;
        }
        
        // Classifica a intenção usando OpenAI
        var classification = await _intentClassifier.ClassifyIntentAsync(request.Text!, session.LastIntent);
        
        // Registra a mensagem com OpenAI
        await _conversationContextService.AddMessageAsync(sessionId, request.Text!, classification.Codes.FirstOrDefault().ToString(), classification.Message, true);
        
        // Atualiza contexto da sessão
        await _conversationContextService.UpdateSessionContextAsync(sessionId, classification.Codes.FirstOrDefault().ToString());

        // Se há códigos para executar, executa o pipeline
        if (classification.Codes.Any())
        {
            var data = new Dictionary<string, object>
            {
                ["phone_number"] = request.From,
                ["message"] = request.Text!
            };

            // Adiciona dados do contexto se existirem
            if (session.ContextData != null)
            {
                foreach (var kvp in session.ContextData)
                {
                    data[kvp.Key] = kvp.Value;
                }
            }

            return await _actionPipeline.ExecutePipelineAsync(classification.Codes, data, request.From);
        }

        // Se não há códigos, retorna a resposta direta da classificação
        return classification;
    }

    /// <summary>
    /// Extrai um valor double de forma segura de diferentes tipos de objetos JSON
    /// </summary>
    private double ExtractDoubleValue(object value)
    {
        if (value == null)
            throw new ArgumentException("Valor não pode ser nulo");

        // Se já é double, retorna diretamente
        if (value is double doubleValue)
            return doubleValue;

        // Se é decimal, converte para double
        if (value is decimal decimalValue)
            return (double)decimalValue;

        // Se é int, converte para double
        if (value is int intValue)
            return intValue;

        // Se é long, converte para double
        if (value is long longValue)
            return longValue;

        // Se é JsonElement, extrai o valor
        if (value.GetType().Name == "JsonElement")
        {
            var jsonElement = (System.Text.Json.JsonElement)value;
            return jsonElement.GetDouble();
        }

        // Tenta converter usando Convert
        try
        {
            return Convert.ToDouble(value);
        }
        catch
        {
            throw new ArgumentException($"Não foi possível converter o valor '{value}' para double");
        }
    }
} 