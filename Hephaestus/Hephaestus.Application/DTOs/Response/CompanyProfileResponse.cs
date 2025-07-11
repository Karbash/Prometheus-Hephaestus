namespace Hephaestus.Application.DTOs.Response
{
    public class CompanyProfileResponse
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string State { get; set; } = string.Empty;
        public string? City { get; set; }
        public string? Street { get; set; }
        public string? Number { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public string? Slogan { get; set; }
        public string? Description { get; set; }
        public List<CompanyImageResponse> Images { get; set; } = new List<CompanyImageResponse>();
        public List<CompanyOperatingHourResponse> OperatingHours { get; set; } = new List<CompanyOperatingHourResponse>();
        public List<CompanySocialMediaResponse> SocialMedia { get; set; } = new List<CompanySocialMediaResponse>();
    }
}
