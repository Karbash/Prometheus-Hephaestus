using Hephaestus.Domain.Enum;

namespace Hephaestus.Domain.Entities;

public class ProductionQueue
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string OrderId { get; set; } = string.Empty;
    public int Priority { get; set; }
    public QueueStatus Status { get; set; }
}

