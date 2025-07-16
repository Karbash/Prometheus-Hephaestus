using Hephaestus.Domain.Enum;

namespace Hephaestus.Domain.Entities;

public class Customization
{
    public CustomizationType Type { get; set; }
    public string Value { get; set; } = string.Empty;
}
