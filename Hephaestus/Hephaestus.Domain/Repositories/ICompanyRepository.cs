using Hephaestus.Domain.Entities;

namespace Hephaestus.Domain.Repositories
{
    public interface ICompanyRepository
    {
        Task<Company?> GetByEmailAsync(string email);
        Task<Company?> GetByPhoneNumberAsync(string phoneNumber);
        Task AddAsync(Company company);
        Task UpdateAsync(Company company);
    }
}
