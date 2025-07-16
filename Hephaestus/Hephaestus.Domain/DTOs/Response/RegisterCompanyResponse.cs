namespace Hephaestus.Domain.DTOs.Response
{
    public class RegisterCompanyResponse
    {
        public RegisterCompanyResponse(string companyId)
        {
            CompanyId = companyId;
        }

        public string CompanyId { get; set; } = string.Empty;
    }
}
