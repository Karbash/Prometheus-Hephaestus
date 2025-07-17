using System;

namespace Hephaestus.Domain.Entities;

/// <summary>
/// Entidade para registrar o uso de uma promoção por um cliente em um pedido.
/// </summary>
public class PromotionUsage
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string CompanyId { get; set; } = string.Empty;
    public string PromotionId { get; set; } = string.Empty;
    public string CustomerId { get; set; } = string.Empty;
    public string OrderId { get; set; } = string.Empty;
    public DateTime UsedAt { get; set; } = DateTime.UtcNow;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
} 