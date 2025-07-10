using Hephaestus.Application.DTOs.Response;

namespace Hephaestus.Application.Interfaces.Administration;

public interface IGetCompaniesUseCase
{
    Task<IEnumerable<CompanyResponse>> ExecuteAsync(bool? isEnabled);
}
