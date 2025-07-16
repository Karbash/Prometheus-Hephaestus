using System.ComponentModel.DataAnnotations;
using Hephaestus.Domain.Enum;

namespace Hephaestus.Domain.DTOs.Request;

/// <summary>
/// DTO para atualização do status de pagamento do pedido.
/// </summary>
public class UpdateOrderPaymentStatusRequest
{
    [Required]
    public PaymentStatus Status { get; set; }
} 
