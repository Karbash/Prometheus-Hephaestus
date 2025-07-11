using FluentValidation;
using Hephaestus.Application.DTOs.Request;
using Hephaestus.Application.Interfaces.OpenAI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;

namespace Hephaestus.Application.UseCases.OpenAI;

public class ChatWithOpenAIUseCase : IChatWithOpenAIUseCase
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly ILogger<ChatWithOpenAIUseCase> _logger;
    private readonly IValidator<OpenAIChatRequest> _validator;

    public ChatWithOpenAIUseCase(HttpClient httpClient, IValidator<OpenAIChatRequest> validator, IConfiguration configuration, ILogger<ChatWithOpenAIUseCase> logger)
    {
        _validator = validator;
        _httpClient = httpClient;
        _apiKey = configuration["OpenAI:ApiKey"] ?? throw new ArgumentNullException("Chave da API OpenAI não configurada.");
        _logger = logger;
    }

    public async Task<OpenAIChatResponse> ExecuteAsync(OpenAIChatRequest request)
    {
        _validator.ValidateAndThrow(request);

        if (string.IsNullOrWhiteSpace(request.Prompt))
            throw new ArgumentException("Prompt é obrigatório.", nameof(request.Prompt));

        var responseFormatInstruction = request.ResponseFormat != null
            ? $"Retorne a resposta no formato JSON conforme o esquema: {JsonSerializer.Serialize(request.ResponseFormat)}"
            : "Retorne a resposta em formato JSON.";

        var fullPrompt = $"{request.Prompt}\nDados: {request.Data}\n{responseFormatInstruction}";

        var responseFormat = request.ResponseFormat != null &&
                             request.ResponseFormat.ContainsKey("type") &&
                             request.ResponseFormat["type"] == "json_object"
            ? new { type = "json_object" }
            : new { type = "text" };

        var payload = new
        {
            model = "gpt-4o-mini",
            messages = new[]
            {
                new { role = "user", content = fullPrompt }
            },
            response_format = responseFormat
        };

        _logger.LogInformation("Enviando payload para OpenAI: {Payload}", JsonSerializer.Serialize(payload));

        var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
        _httpClient.DefaultRequestHeaders.Authorization = new("Bearer", _apiKey);

        var response = await _httpClient.PostAsync("https://api.openai.com/v1/chat/completions", content);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogError("Erro na API da OpenAI: {StatusCode}. Detalhes: {ErrorContent}", response.StatusCode, errorContent);
            throw new HttpRequestException($"Erro na API da OpenAI: {response.StatusCode}. Detalhes: {errorContent}");
        }

        var responseJson = await response.Content.ReadAsStringAsync();
        _logger.LogInformation("Resposta bruta da OpenAI: {ResponseJson}", responseJson);

        var openAIResponse = JsonSerializer.Deserialize<OpenAIResponse>(
            responseJson,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
        );

        if (openAIResponse?.Choices == null || openAIResponse.Choices.Length == 0)
        {
            _logger.LogWarning("Nenhuma escolha retornada pela OpenAI.");
            return new OpenAIChatResponse { ResponseJson = null };
        }

        var rawContent = openAIResponse.Choices.FirstOrDefault()?.Message?.Content ?? string.Empty;

        // Remove blocos ```json, ``` e qualquer texto antes ou depois do JSON
        var jsonStart = rawContent.IndexOf('{');
        var jsonEnd = rawContent.LastIndexOf('}');

        if (jsonStart < 0 || jsonEnd < 0 || jsonEnd <= jsonStart)
        {
            _logger.LogError("Resposta da OpenAI com formato inválido.");
            throw new HttpRequestException("Resposta da OpenAI com formato inválido.");
        }

        var cleanContent = rawContent.Substring(jsonStart, jsonEnd - jsonStart + 1).Trim();

        _logger.LogInformation("Resposta JSON extraída da OpenAI: {CleanContent}", cleanContent);

        Dictionary<string, string>? parsedJson = null;
        try
        {
            parsedJson = JsonSerializer.Deserialize<Dictionary<string, string>>(cleanContent);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Erro ao desserializar a resposta da OpenAI");
            throw new HttpRequestException("Resposta da OpenAI com formato inválido.");
        }

        return new OpenAIChatResponse
        {
            ResponseJson = parsedJson
        };
    }

    private class OpenAIResponse
    {
        public OpenAIChoice[]? Choices { get; set; }
    }

    private class OpenAIChoice
    {
        public OpenAIMessage? Message { get; set; }
    }

    private class OpenAIMessage
    {
        public string Content { get; set; } = string.Empty;
    }
}
