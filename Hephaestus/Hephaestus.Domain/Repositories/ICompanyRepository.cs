using Hephaestus.Domain.Entities;

namespace Hephaestus.Domain.Repositories;

public interface ICompanyRepository
{
    Task<IEnumerable<Company>> GetAllAsync(bool? isEnabled);
    Task<Company?> GetByIdAsync(string id);
    Task<Company?> GetByEmailAsync(string email);
    Task<Company?> GetByPhoneNumberAsync(string phoneNumber);
    Task AddAsync(Company company);
    Task UpdateAsync(Company company);
}