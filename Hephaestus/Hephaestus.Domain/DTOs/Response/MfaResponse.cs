namespace Hephaestus.Domain.DTOs.Response
{
    public class MfaResponse
    {
        public string Token { get; set; }

        public MfaResponse(string token)
        {
            Token = token;
        }
    }
}
