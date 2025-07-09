using Hephaestus.Domain.Entities;

namespace Hephaestus.Application.DTOs.Request
{
    public class RegisterCompanyRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string ApiKey { get; set; } = string.Empty;
        public FeeType FeeType { get; set; } = FeeType.Fixed;
        public decimal FeeValue { get; set; } = 0;
        public bool IsEnabled { get; set; } = false;
    }
}
