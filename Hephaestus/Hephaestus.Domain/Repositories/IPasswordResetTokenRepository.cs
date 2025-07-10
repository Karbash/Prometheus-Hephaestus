using Hephaestus.Domain.Entities;

namespace Hephaestus.Domain.Repositories;

public interface IPasswordResetTokenRepository
{
    Task AddAsync(PasswordResetToken token);
    Task<PasswordResetToken?> GetByEmailAndTokenAsync(string email, string token);
    Task DeleteAsync(PasswordResetToken token);
}