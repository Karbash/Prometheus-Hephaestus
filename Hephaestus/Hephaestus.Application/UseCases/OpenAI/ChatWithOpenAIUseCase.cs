using Hephaestus.Application.DTOs.Request;
using Hephaestus.Application.DTOs.Response;
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
/// Caso de uso para comunicação com a API do OpenAI.
/// </summary>
public class ChatWithOpenAIUseCase : BaseUseCase, IChatWithOpenAIUseCase
{
    private readonly IConfiguration _configuration;
    private readonly HttpClient _httpClient;

    /// <summary>
    /// Inicializa uma nova instância do <see cref="ChatWithOpenAIUseCase"/>.
    /// </summary>
    /// <param name="configuration">Configuração da aplicação.</param>
    /// <param name="httpClient">Cliente HTTP.</param>
    /// <param name="logger">Logger.</param>
    /// <param name="exceptionHandler">Serviço de tratamento de exceções.</param>
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
    /// Executa a comunicação com a API do OpenAI.
    /// </summary>
    /// <param name="request">Dados da requisição para o OpenAI.</param>
    /// <returns>Resposta do OpenAI.</returns>
    public async Task<OpenAIResponse> ExecuteAsync(OpenAIRequest request)
    {
        return await ExecuteWithExceptionHandlingAsync(async () =>
        {
            // Validação dos dados de entrada
            ValidateRequest(request);

            // Validação da configuração
            ValidateConfiguration();

            // Preparação da requisição
            var httpRequest = await PrepareHttpRequestAsync(request);

            // Execução da requisição
            var response = await ExecuteHttpRequestAsync(httpRequest);

            // Processamento da resposta
            return await ProcessResponseAsync(response);
        }, "Comunicação com OpenAI");
    }

    /// <summary>
    /// Valida os dados da requisição.
    /// </summary>
    /// <param name="request">Requisição a ser validada.</param>
    private void ValidateRequest(OpenAIRequest request)
    {
        if (request == null)
            throw new Hephaestus.Application.Exceptions.ValidationException("Dados da requisição são obrigatórios.", new ValidationResult());

        if (string.IsNullOrEmpty(request.Prompt))
            throw new Hephaestus.Application.Exceptions.ValidationException("Prompt é obrigatório.", new ValidationResult());

        if (request.Prompt.Length > 4000)
            throw new Hephaestus.Application.Exceptions.ValidationException("Prompt muito longo. Máximo de 4.000 caracteres permitido.", new ValidationResult());
    }

    /// <summary>
    /// Valida a configuração necessária.
    /// </summary>
    private void ValidateConfiguration()
    {
        var apiKey = _configuration["OpenAI:ApiKey"];
        if (string.IsNullOrEmpty(apiKey))
            throw new BusinessRuleException("Chave da API do OpenAI não configurada.", "OPENAI_CONFIG");

        var baseUrl = _configuration["OpenAI:BaseUrl"];
        if (string.IsNullOrEmpty(baseUrl))
            throw new BusinessRuleException("URL base do OpenAI não configurada.", "OPENAI_CONFIG");
    }

    /// <summary>
    /// Prepara a requisição HTTP.
    /// </summary>
    /// <param name="request">Dados da requisição.</param>
    /// <returns>Requisição HTTP preparada.</returns>
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
    /// Executa a requisição HTTP.
    /// </summary>
    /// <param name="httpRequest">Requisição HTTP.</param>
    /// <returns>Resposta HTTP.</returns>
    private async Task<HttpResponseMessage> ExecuteHttpRequestAsync(HttpRequestMessage httpRequest)
    {
        try
        {
            return await _httpClient.SendAsync(httpRequest);
        }
        catch (HttpRequestException ex)
        {
            throw new BusinessRuleException($"Erro na comunicação com a API do OpenAI: {ex.Message}", "OPENAI_COMMUNICATION");
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
        var openAIResponse = JsonSerializer.Deserialize<dynamic>(responseContent);

        // Processa a resposta e converte para o formato esperado
        var responseData = new Dictionary<string, string>();
        
        try
        {
            // Extrai a resposta do modelo
            var choices = openAIResponse?.GetProperty("choices");
            if (choices != null && choices.GetArrayLength() > 0)
            {
                var firstChoice = choices[0];
                var message = firstChoice?.GetProperty("message");
                var content = message?.GetProperty("content")?.GetString();
                
                if (!string.IsNullOrEmpty(content))
                {
                    responseData["response"] = content;
                }
            }
        }
        catch
        {
            throw new BusinessRuleException("Resposta inválida da API do OpenAI.", "OPENAI_RESPONSE_INVALID");
        }

        return new OpenAIResponse
        {
            ResponseJson = responseData
        };
    }
}
