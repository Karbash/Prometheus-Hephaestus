namespace Hephaestus.Domain.DTOs.Request
{
    public class MfaRequest
    {
        public string Email { get; set; } = string.Empty;
        public string MfaCode { get; set; } = string.Empty;
    }
}
