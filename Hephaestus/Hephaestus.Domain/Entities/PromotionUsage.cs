using System;

namespace Hephaestus.Domain.Entities;

/// <summary>
/// Entidade para registrar o uso de uma promoção por um cliente em um pedido.
/// </summary>
public class PromotionUsage
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string TenantId { get; set; } = string.Empty;
    public string PromotionId { get; set; } = string.Empty;
    public string CustomerPhoneNumber { get; set; } = string.Empty;
    public string OrderId { get; set; } = string.Empty;
    public DateTime UsedAt { get; set; } = DateTime.UtcNow;
} 
