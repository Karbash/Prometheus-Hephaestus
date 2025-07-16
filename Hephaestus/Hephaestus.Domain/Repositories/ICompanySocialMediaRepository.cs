using Hephaestus.Domain.Entities;

namespace Hephaestus.Domain.Repositories;

public interface ICompanySocialMediaRepository
{
    Task<IEnumerable<CompanySocialMedia>> GetByCompanyIdAsync(string companyId);
}
