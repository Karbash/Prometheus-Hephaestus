using Hephaestus.Domain.Enum;
using System.ComponentModel.DataAnnotations;

namespace Hephaestus.Domain.DTOs.Request;

/// <summary>
/// DTO para atualização do status de pagamento.
/// </summary>
public class UpdatePaymentStatusRequest
{
    /// <summary>
    /// Novo status do pagamento. Valores possíveis: Pending, Paid, Failed, Refunded, Processed.
    /// </summary>
    [Required]
    public PaymentStatus Status { get; set; }
} 
