using Hephaestus.Domain.DTOs.Response;

namespace Hephaestus.Application.Interfaces.Company;

public interface IGetCompanyProfileUseCase
{
    Task<CompanyProfileResponse> ExecuteAsync(string companyId);
}
