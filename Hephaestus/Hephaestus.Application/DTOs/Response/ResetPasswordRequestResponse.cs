namespace Hephaestus.Application.DTOs.Response
{
    public class ResetPasswordRequestResponse
    {
        public string ResetToken { get; set; }

        public ResetPasswordRequestResponse(string resetToken)
        {
            ResetToken = resetToken;
        }
    }
}
