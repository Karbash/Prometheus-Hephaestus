namespace Hephaestus.Domain.DTOs.Request
{
    public class ResetPasswordConfirmRequest
    {
        public string Email { get; set; } = string.Empty;
        public string ResetToken { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
    }
}
