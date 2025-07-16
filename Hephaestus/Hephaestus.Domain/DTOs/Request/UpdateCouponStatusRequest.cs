using System.ComponentModel.DataAnnotations;

namespace Hephaestus.Domain.DTOs.Request;

/// <summary>
/// DTO para atualização do status de ativação do cupom.
/// </summary>
public class UpdateCouponStatusRequest
{
    /// <summary>
    /// Define se o cupom estará ativo (true) ou inativo (false).
    /// </summary>
    [Required]
    public bool IsActive { get; set; }
} 
