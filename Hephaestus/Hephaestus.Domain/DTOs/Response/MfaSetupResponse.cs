namespace Hephaestus.Application.DTOs.Response
{
    public class MfaSetupResponse
    {
        public string Secret { get; set; }

        public MfaSetupResponse(string secret)
        {
            Secret = secret ?? throw new ArgumentNullException(nameof(secret));
        }
    }
}
