using Hephaestus.Domain.Entities;
using Hephaestus.Domain.Repositories;
using Hephaestus.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Hephaestus.Infrastructure.Repositories;

public class CompanySocialMediaRepository : ICompanySocialMediaRepository
{
    private readonly HephaestusDbContext _context;

    public CompanySocialMediaRepository(HephaestusDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<CompanySocialMedia>> GetByCompanyIdAsync(string companyId)
    {
        return await _context.CompanySocialMedia
            .AsNoTracking()
            .Where(sm => sm.CompanyId == companyId)
            .ToListAsync();
    }
}
