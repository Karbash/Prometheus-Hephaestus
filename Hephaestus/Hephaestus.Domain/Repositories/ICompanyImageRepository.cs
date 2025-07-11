using Hephaestus.Domain.Entities;

namespace Hephaestus.Domain.Repositories;

public interface ICompanyImageRepository
{
    Task<IEnumerable<CompanyImage>> GetByCompanyIdAsync(string companyId);
}