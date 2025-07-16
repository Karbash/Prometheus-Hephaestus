namespace Hephaestus.Domain.DTOs.Response
{
    public class LoginResponse
    {
        public LoginResponse(string token)
        {
            Token = token;
        }

        public string Token { get; set; } = string.Empty;
    }
}
