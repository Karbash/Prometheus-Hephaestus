using System.ComponentModel.DataAnnotations;

namespace Hephaestus.Domain.DTOs.Request;

/// <summary>
/// DTO para registrar o uso de um cupom.
/// </summary>
public class UseCouponRequest
{
    /// <summary>
    /// Número de telefone do cliente que está usando o cupom.
    /// </summary>
    [Required]
    public string CustomerPhoneNumber { get; set; } = string.Empty;

    /// <summary>
    /// ID do pedido associado ao uso do cupom.
    /// </summary>
    [Required]
    public string OrderId { get; set; } = string.Empty;
} 
