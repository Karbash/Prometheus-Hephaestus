namespace Hephaestus.Domain.Entities;

public class CompanySocialMedia
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string CompanyId { get; set; } = string.Empty;
    public string Platform { get; set; } = string.Empty; // Ex.: "Instagram", "Facebook"
    public string Url { get; set; } = string.Empty;
}
