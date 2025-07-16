using Hephaestus.Domain.Enum;
using System.ComponentModel.DataAnnotations;

namespace Hephaestus.Domain.DTOs.Request;

/// <summary>
/// DTO para atualização do status do pedido.
/// </summary>
public class UpdateOrderStatusRequest
{
    /// <summary>
    /// Novo status do pedido. Valores possíveis: Pending, InProduction, Completed, Cancelled.
    /// </summary>
    [Required]
    public OrderStatus Status { get; set; }
} 
