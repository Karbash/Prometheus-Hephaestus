using Hephaestus.Domain.Entities;
using Hephaestus.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace Hephaestus.Infrastructure.Data;

public class PasswordResetTokenRepository : IPasswordResetTokenRepository
{
    private readonly HephaestusDbContext _context;

    public PasswordResetTokenRepository(HephaestusDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(PasswordResetToken token)
    {
        await _context.PasswordResetTokens.AddAsync(token);
        await _context.SaveChangesAsync();
    }

    public async Task<PasswordResetToken?> GetByEmailAndTokenAsync(string email, string token)
    {
        return await _context.PasswordResetTokens
            .FirstOrDefaultAsync(t => t.Email == email && t.Token == token);
    }

    public async Task DeleteAsync(PasswordResetToken token)
    {
        _context.PasswordResetTokens.Remove(token);
        await _context.SaveChangesAsync();
    }
}
