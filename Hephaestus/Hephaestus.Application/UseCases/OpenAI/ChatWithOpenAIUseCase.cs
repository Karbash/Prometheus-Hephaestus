using Hephaestus.Domain.DTOs.Request;
using Hephaestus.Domain.DTOs.Response;
using Hephaestus.Application.Interfaces.OpenAI;
using Hephaestus.Application.Exceptions;
using Hephaestus.Application.Base;
using Microsoft.Extensions.Logging;
using Hephaestus.Application.Services;
using FluentValidation.Results;
using Microsoft.Extensions.Configuration;
using System.Text;
using System.Text.Json;

namespace Hephaestus.Application.UseCases.OpenAI;

/// <summary>
/// Caso de uso para comunica��o com a API do OpenAI.
/// </summary>
public class ChatWithOpenAIUseCase : BaseUseCase, IChatWithOpenAIUseCase
{
    private readonly IConfiguration _configuration;
    private readonly HttpClient _httpClient;

    /// <summary>
    /// Inicializa uma nova inst�ncia do <see cref="ChatWithOpenAIUseCase"/>.
    /// </summary>
    /// <param name="configuration">Configura��o da aplica��o.</param>
    /// <param name="httpClient">Cliente HTTP.</param>
    /// <param name="logger">Logger.</param>
    /// <param name="exceptionHandler">Servi�o de tratamento de exce��es.</param>
    public ChatWithOpenAIUseCase(
        IConfiguration configuration, 
        HttpClient httpClient,
        ILogger<ChatWithOpenAIUseCase> logger,
        IExceptionHandlerService exceptionHandler)
        : base(logger, exceptionHandler)
    {
        _configuration = configuration;
        _httpClient = httpClient;
    }

    /// <summary>
    /// Executa a comunica��o com a API do OpenAI.
    /// </summary>
    /// <param name="request">Dados da requisi��o para o OpenAI.</param>
    /// <returns>Resposta do OpenAI.</returns>
    public async Task<OpenAIResponse> ExecuteAsync(OpenAIRequest request)
    {
        return await ExecuteWithExceptionHandlingAsync(async () =>
        {
            // Valida��o dos dados de entrada
            ValidateRequest(request);

            // Valida��o da configura��o
            ValidateConfiguration();

            // Prepara��o da requisi��o
            var httpRequest = await PrepareHttpRequestAsync(request);

            // Execu��o da requisi��o
            var response = await ExecuteHttpRequestAsync(httpRequest);

            // Processamento da resposta
            return await ProcessResponseAsync(response);
        }, "Comunica��o com OpenAI");
    }

    /// <summary>
    /// Valida os dados da requisi��o.
    /// </summary>
    /// <param name="request">Requisi��o a ser validada.</param>
    private void ValidateRequest(OpenAIRequest request)
    {
        if (request == null)
            throw new Hephaestus.Application.Exceptions.ValidationException("Dados da requisi��o s�o obrigat�rios.", new ValidationResult());

        if (string.IsNullOrEmpty(request.Prompt))
            throw new Hephaestus.Application.Exceptions.ValidationException("Prompt � obrigat�rio.", new ValidationResult());

        if (request.Prompt.Length > 4000)
            throw new Hephaestus.Application.Exceptions.ValidationException("Prompt muito longo. M�ximo de 4.000 caracteres permitido.", new ValidationResult());
    }

    /// <summary>
    /// Valida a configura��o necess�ria.
    /// </summary>
    private void ValidateConfiguration()
    {
        var apiKey = _configuration["OpenAI:ApiKey"];
        if (string.IsNullOrEmpty(apiKey))
            throw new BusinessRuleException("Chave da API do OpenAI n�o configurada.", "OPENAI_CONFIG");

        var baseUrl = _configuration["OpenAI:BaseUrl"];
        if (string.IsNullOrEmpty(baseUrl))
            throw new BusinessRuleException("URL base do OpenAI n�o configurada.", "OPENAI_CONFIG");
    }

    /// <summary>
    /// Prepara a requisi��o HTTP.
    /// </summary>
    /// <param name="request">Dados da requisi��o.</param>
    /// <returns>Requisi��o HTTP preparada.</returns>
    private Task<HttpRequestMessage> PrepareHttpRequestAsync(OpenAIRequest request)
    {
        var apiKey = _configuration["OpenAI:ApiKey"];
        var baseUrl = _configuration["OpenAI:BaseUrl"];

        var httpRequest = new HttpRequestMessage(HttpMethod.Post, $"{baseUrl}/v1/chat/completions");
        httpRequest.Headers.Add("Authorization", $"Bearer {apiKey}");

        var requestBody = new
        {
            model = "gpt-3.5-turbo",
            messages = new[]
            {
                new { role = "user", content = request.Prompt }
            },
            max_tokens = 1000,
            temperature = 0.7
        };

        var jsonContent = JsonSerializer.Serialize(requestBody);
        httpRequest.Content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

        return Task.FromResult(httpRequest);
    }

    /// <summary>
    /// Executa a requisi��o HTTP.
    /// </summary>
    /// <param name="httpRequest">Requisi��o HTTP.</param>
    /// <returns>Resposta HTTP.</returns>
    private async Task<HttpResponseMessage> ExecuteHttpRequestAsync(HttpRequestMessage httpRequest)
    {
        try
        {
            return await _httpClient.SendAsync(httpRequest);
        }
        catch (HttpRequestException ex)
        {
            throw new BusinessRuleException($"Erro na comunica��o com a API do OpenAI: {ex.Message}", "OPENAI_COMMUNICATION");
        }
    }

    /// <summary>
    /// Processa a resposta da API.
    /// </summary>
    /// <param name="response">Resposta HTTP.</param>
    /// <returns>Resposta processada.</returns>
    private async Task<OpenAIResponse> ProcessResponseAsync(HttpResponseMessage response)
    {
        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new BusinessRuleException($"Erro na API do OpenAI: {response.StatusCode} - {errorContent}", "OPENAI_API_ERROR");
        }

        var responseContent = await response.Content.ReadAsStringAsync();
        var responseData = new Dictionary<string, string>();

        try
        {
            using var doc = JsonDocument.Parse(responseContent);
            var root = doc.RootElement;
            if (root.TryGetProperty("choices", out var choices) && choices.ValueKind == JsonValueKind.Array && choices.GetArrayLength() > 0)
            {
                var firstChoice = choices[0];
                if (firstChoice.TryGetProperty("message", out var message) && message.TryGetProperty("content", out var contentElement))
                {
                    var content = contentElement.GetString();
                    if (!string.IsNullOrEmpty(content))
                    {
                        responseData["response"] = content;
                    }
                }
                // Suporte para resposta no formato antigo (apenas 'text')
                else if (firstChoice.TryGetProperty("text", out var textElement))
                {
                    var text = textElement.GetString();
                    if (!string.IsNullOrEmpty(text))
                    {
                        responseData["response"] = text;
                    }
                }
            }
            else
            {
                throw new BusinessRuleException("Resposta da API do OpenAI não contém o campo 'choices' ou está vazia.", "OPENAI_RESPONSE_INVALID");
            }
        }
        catch (JsonException ex)
        {
            throw new BusinessRuleException($"Falha ao processar JSON da resposta do OpenAI: {ex.Message}", "OPENAI_RESPONSE_INVALID");
        }
        catch (Exception ex)
        {
            throw new BusinessRuleException($"Erro inesperado ao processar resposta do OpenAI: {ex.Message}", "OPENAI_RESPONSE_INVALID");
        }

        return new OpenAIResponse
        {
            ResponseJson = responseData
        };
    }
}
