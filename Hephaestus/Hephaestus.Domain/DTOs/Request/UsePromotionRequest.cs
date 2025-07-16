using System.ComponentModel.DataAnnotations;

namespace Hephaestus.Domain.DTOs.Request;

/// <summary>
/// DTO para registrar o uso de uma promoção.
/// </summary>
public class UsePromotionRequest
{
    /// <summary>
    /// Número de telefone do cliente que está usando a promoção.
    /// </summary>
    [Required]
    public string CustomerPhoneNumber { get; set; } = string.Empty;

    /// <summary>
    /// ID do pedido associado ao uso da promoção.
    /// </summary>
    [Required]
    public string OrderId { get; set; } = string.Empty;
} 
