namespace Hephaestus.Domain.DTOs.Response
{
    public class TagResponse
    {
        public string Id { get; set; } = string.Empty;
        public string CompanyId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool IsGlobal { get; set; } = false; // Indica se é uma tag global (criada por admin)
    }
}
