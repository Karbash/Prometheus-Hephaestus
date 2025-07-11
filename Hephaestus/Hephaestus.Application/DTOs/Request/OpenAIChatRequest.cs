namespace Hephaestus.Application.DTOs.Request;

public class OpenAIChatRequest
{
    public string Prompt { get; set; } = string.Empty;
    public string? Data { get; set; }  // opcional
    public Dictionary<string, string>? ResponseFormat { get; set; }  // opcional
}