namespace Hephaestus.Domain.DTOs.Response
{
    public class CompanyProfileResponse
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public AddressResponse? Address { get; set; }
        public string? Slogan { get; set; }
        public string? Description { get; set; }
        public List<CompanyImageResponse> Images { get; set; } = new List<CompanyImageResponse>();
        public List<CompanyOperatingHourResponse> OperatingHours { get; set; } = new List<CompanyOperatingHourResponse>();
        public List<CompanySocialMediaResponse> SocialMedia { get; set; } = new List<CompanySocialMediaResponse>();
    }
}
