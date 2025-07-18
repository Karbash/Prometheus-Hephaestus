namespace Hephaestus.Domain.DTOs.Request;

/// <summary>
/// Requisição para o endpoint OpenAI/chat.
/// </summary>
public class OpenAIRequest
{
    /// <summary>
    /// Prompt a ser enviado ao modelo da OpenAI.
    /// </summary>
    public string Prompt { get; set; } = string.Empty;

    /// <summary>
    /// Dados adicionais (opcional).
    /// </summary>
    public string? Data { get; set; }  // opcional

    /// <summary>
    /// Formato desejado para a resposta.
    /// <para>Exemplo de uso:</para>
    /// <code>
    /// {
    ///   "type": "json_object", // ou "text"
    ///   "campo1": "tipo1",
    ///   "campo2": "tipo2"
    /// }
    /// </code>
    /// <para>
    /// O campo <c>type</c> pode ser:
    /// <list type="bullet">
    ///   <item><description><c>text</c>: resposta simples em texto</description></item>
    ///   <item><description><c>json_object</c>: resposta estruturada em JSON, com os campos adicionais especificados</description></item>
    /// </list>
    /// Os demais campos (além de <c>type</c>) definem o nome e o tipo esperado de cada campo no JSON de resposta.
    /// </para>
    /// <para>Exemplo para resposta estruturada:</para>
    /// <code>
    /// {
    ///   "type": "json_object",
    ///   "historia": "resumo",
    ///   "idade": "numero"
    /// }
    /// </code>
    /// </summary>
    public Dictionary<string, string>? ResponseFormat { get; set; }  // opcional
}
