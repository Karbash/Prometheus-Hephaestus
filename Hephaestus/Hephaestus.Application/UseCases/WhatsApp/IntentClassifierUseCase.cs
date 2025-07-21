using Hephaestus.Application.Base;
using Hephaestus.Application.Interfaces.WhatsApp;
using Hephaestus.Application.Interfaces.OpenAI;
using Hephaestus.Application.Services;
using Hephaestus.Domain.DTOs.Request;
using Hephaestus.Domain.DTOs.Response;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Hephaestus.Application.UseCases.WhatsApp;

/// <summary>
/// Classificador de intenções que decide quando usar OpenAI ou endpoints existentes
/// </summary>
public class IntentClassifierUseCase : BaseUseCase, IIntentClassifierUseCase
{
    private readonly IChatWithOpenAIUseCase _openAIUseCase;

    public IntentClassifierUseCase(
        IChatWithOpenAIUseCase openAIUseCase,
        ILogger<IntentClassifierUseCase> logger,
        IExceptionHandlerService exceptionHandler)
        : base(logger, exceptionHandler)
    {
        _openAIUseCase = openAIUseCase;
    }

    public async Task<WhatsAppResponse> ClassifyIntentAsync(string message, string? conversationContext = null)
    {
        return await ExecuteWithExceptionHandlingAsync(async () =>
        {
            // Primeiro, tenta classificar com regras simples (sem OpenAI)
            var simpleClassification = ClassifyWithSimpleRules(message);
            if (simpleClassification != null)
            {
                return simpleClassification;
            }

            // Se não conseguiu classificar com regras simples, usa OpenAI
            return await ClassifyWithOpenAIAsync(message, conversationContext);
        }, "IntentClassification");
    }

    private WhatsAppResponse? ClassifyWithSimpleRules(string message)
    {
        var lowerMessage = message.ToLower();

        // Buscar restaurantes próximos
        if (lowerMessage.Contains("restaurante") && (lowerMessage.Contains("próximo") || lowerMessage.Contains("perto") || lowerMessage.Contains("perto de")))
        {
            return new WhatsAppResponse
            {
                Message = "Para encontrar restaurantes próximos, preciso da sua localização. Por favor, compartilhe sua localização atual.",
                Codes = new List<int> { 1001 }, // Código para buscar restaurantes próximos
                Data = new Dictionary<string, object>
                {
                    ["waiting_for_location"] = true,
                    ["pending_codes"] = new List<int> { 1001 }
                },
                WaitForResponse = true,
                ConversationContext = "waiting_location_for_restaurants"
            };
        }

        // Cardápio
        if (lowerMessage.Contains("cardápio") || lowerMessage.Contains("menu") || lowerMessage.Contains("pratos"))
        {
            return new WhatsAppResponse
            {
                Message = "Vou buscar o cardápio para você. Qual tipo de comida você prefere?",
                Codes = new List<int> { 2001 }, // Código para buscar cardápio
                WaitForResponse = true,
                ConversationContext = "menu_selection"
            };
        }

        // Horário de funcionamento
        if (lowerMessage.Contains("horário") || lowerMessage.Contains("funcionamento") || lowerMessage.Contains("aberto"))
        {
            return new WhatsAppResponse
            {
                Message = "Vou verificar os horários de funcionamento dos restaurantes próximos.",
                Codes = new List<int> { 3001 }, // Código para buscar horários
                WaitForResponse = false
            };
        }

        // Pedido
        if (lowerMessage.Contains("pedido") || lowerMessage.Contains("fazer pedido") || lowerMessage.Contains("comprar"))
        {
            return new WhatsAppResponse
            {
                Message = "Para fazer um pedido, preciso saber qual restaurante você quer. Vou mostrar as opções próximas.",
                Codes = new List<int> { 4001 }, // Código para iniciar pedido
                WaitForResponse = true,
                ConversationContext = "order_selection"
            };
        }

        // Promoções
        if (lowerMessage.Contains("promoção") || lowerMessage.Contains("desconto") || lowerMessage.Contains("oferta"))
        {
            return new WhatsAppResponse
            {
                Message = "Vou verificar as promoções disponíveis nos restaurantes próximos.",
                Codes = new List<int> { 5001 }, // Código para buscar promoções
                WaitForResponse = false
            };
        }

        // Se não encontrou padrão simples, retorna null para usar OpenAI
        return null;
    }

    private async Task<WhatsAppResponse> ClassifyWithOpenAIAsync(string message, string? conversationContext = null)
    {
        var prompt = BuildClassificationPrompt(message, conversationContext);

        var openAIRequest = new OpenAIRequest
        {
            Prompt = prompt,
            ResponseFormat = new Dictionary<string, string>
            {
                ["type"] = "json_object",
                ["message"] = "string",
                ["codes"] = "string", // Lista de códigos separados por vírgula
                ["wait_for_response"] = "boolean",
                ["conversation_context"] = "string"
            }
        };

        var openAIResponse = await _openAIUseCase.ExecuteAsync(openAIRequest);

        // Processa a resposta da OpenAI
        if (openAIResponse.ResponseJson.TryGetValue("response", out var responseObj))
        {
            if (responseObj is JsonElement jsonElement)
            {
                var responseMessage = jsonElement.GetProperty("message").GetString() ?? "Desculpe, não entendi sua solicitação.";
                var codesStr = jsonElement.GetProperty("codes").GetString() ?? "";
                var waitForResponse = jsonElement.GetProperty("wait_for_response").GetBoolean();
                var context = jsonElement.GetProperty("conversation_context").GetString();

                // Converte string de códigos em lista
                var codes = new List<int>();
                if (!string.IsNullOrEmpty(codesStr))
                {
                    codes = codesStr.Split(',')
                        .Select(c => c.Trim())
                        .Where(c => int.TryParse(c, out _))
                        .Select(int.Parse)
                        .ToList();
                }

                return new WhatsAppResponse
                {
                    Message = responseMessage,
                    Codes = codes,
                    WaitForResponse = waitForResponse,
                    ConversationContext = context
                };
            }
        }

        // Fallback se não conseguir processar resposta da OpenAI
        return new WhatsAppResponse
        {
            Message = "Desculpe, não consegui entender sua solicitação. Pode reformular?",
            WaitForResponse = true
        };
    }

    private string BuildClassificationPrompt(string message, string? conversationContext)
    {
        var prompt = @"Você é um assistente de WhatsApp para um sistema de delivery de comida. 

Analise a mensagem do usuário e retorne um JSON com:
- message: resposta direta ao usuário
- codes: lista de códigos de ações a serem executadas (separados por vírgula)
- wait_for_response: se deve aguardar resposta do usuário
- conversation_context: contexto para próxima interação

CÓDIGOS DISPONÍVEIS:
1001 - Buscar restaurantes próximos (requer localização)
2001 - Buscar cardápio/menu
2002 - Buscar itens do menu por categoria
3001 - Verificar horários de funcionamento
4001 - Iniciar processo de pedido
5001 - Buscar promoções/descontos
5002 - Buscar cupons
6001 - Buscar por tipo de culinária
6002 - Buscar restaurantes por tags/culinária
7001 - Verificar status de pedido
8001 - Cancelar pedido
9001 - Falar com atendente humano

CONTEXTO ATUAL: " + (conversationContext ?? "nenhum") + @"

MENSAGEM DO USUÁRIO: " + message + @"

Exemplo de resposta:
{
  ""message"": ""Para encontrar restaurantes próximos, preciso da sua localização. Pode compartilhar?"",
  ""codes"": ""1001"",
  ""wait_for_response"": true,
  ""conversation_context"": ""waiting_location""
}";

        return prompt;
    }
} 