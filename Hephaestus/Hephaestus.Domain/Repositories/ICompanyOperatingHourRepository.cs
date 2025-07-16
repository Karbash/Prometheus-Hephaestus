using Hephaestus.Domain.Entities;

namespace Hephaestus.Domain.Repositories;

public interface ICompanyOperatingHourRepository
{
    Task<IEnumerable<CompanyOperatingHour>> GetByCompanyIdAsync(string companyId);
}
