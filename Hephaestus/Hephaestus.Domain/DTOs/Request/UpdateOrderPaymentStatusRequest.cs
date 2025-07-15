using Hephaestus.Domain.Enum;
using System.ComponentModel.DataAnnotations;

namespace Hephaestus.Domain.DTOs.Request;

/// <summary>
/// DTO para atualização do status de pagamento do pedido.
/// </summary>
public class UpdateOrderPaymentStatusRequest
{
    /// <summary>
    /// Novo status de pagamento do pedido. Valores possíveis: Pending, Paid, Failed, Refunded, Processed.
    /// </summary>
    [Required]
    public PaymentStatus PaymentStatus { get; set; }
} 