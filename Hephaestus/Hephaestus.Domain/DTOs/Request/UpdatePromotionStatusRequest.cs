using System.ComponentModel.DataAnnotations;

namespace Hephaestus.Domain.DTOs.Request;

/// <summary>
/// DTO para atualização do status de ativação da promoção.
/// </summary>
public class UpdatePromotionStatusRequest
{
    /// <summary>
    /// Define se a promoção estará ativa (true) ou inativa (false).
    /// </summary>
    [Required]
    public bool IsActive { get; set; }
} 