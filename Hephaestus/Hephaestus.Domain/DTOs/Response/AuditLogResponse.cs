using System;

namespace Hephaestus.Application.DTOs.Response;

public class AuditLogResponse
{
    public string Id { get; set; } = string.Empty;
    public string? UserId { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public string Details { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}