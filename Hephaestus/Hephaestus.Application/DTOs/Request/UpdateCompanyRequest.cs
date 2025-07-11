using Hephaestus.Domain.Enum;

namespace Hephaestus.Application.DTOs.Request;

/// <summary>
/// DTO para solicitação de atualização de empresa.
/// </summary>
public class UpdateCompanyRequest
{
    /// <summary>
    /// Identificador único da empresa a ser atualizada.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Nome da empresa (opcional).
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// E-mail da empresa (opcional).
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// Número de telefone da empresa (opcional).
    /// </summary>
    public string? PhoneNumber { get; set; }

    /// <summary>
    /// Chave API da empresa (opcional).
    /// </summary>
    public string? ApiKey { get; set; }

    /// <summary>
    /// Tipo de taxa aplicada à empresa (opcional).
    /// </summary>
    public FeeType? FeeType { get; set; }

    /// <summary>
    /// Valor da taxa aplicada à empresa (opcional).
    /// </summary>
    public decimal? FeeValue { get; set; }

    /// <summary>
    /// Indica se a empresa está ativa (opcional).
    /// </summary>
    public bool? IsEnabled { get; set; }

    /// <summary>
    /// Cidade da empresa (opcional).
    /// </summary>
    public string? City { get; set; }

    /// <summary>
    /// Rua do endereço da empresa (opcional).
    /// </summary>
    public string? Street { get; set; }

    /// <summary>
    /// Número do endereço da empresa (opcional).
    /// </summary>
    public string? Number { get; set; }

    /// <summary>
    /// Latitude da localização da empresa (opcional).
    /// </summary>
    public double? Latitude { get; set; }

    /// <summary>
    /// Longitude da localização da empresa (opcional).
    /// </summary>
    public double? Longitude { get; set; }

    /// <summary>
    /// Slogan da empresa (opcional).
    /// </summary>
    public string? Slogan { get; set; }

    /// <summary>
    /// Descrição da empresa (opcional).
    /// </summary>
    public string? Description { get; set; }
}