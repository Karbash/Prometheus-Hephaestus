﻿namespace Hephaestus.Domain.Entities;

public class AuditLog
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string? TenantId { get; set; }
    public string? CompanyId { get; set; }
    public string? UserId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public string Details { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}