using Hephaestus.Domain.Entities;
using Hephaestus.Domain.Repositories;
using Hephaestus.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Hephaestus.Infrastructure.Repositories;

public class CompanyImageRepository : ICompanyImageRepository
{
    private readonly HephaestusDbContext _context;

    public CompanyImageRepository(HephaestusDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<CompanyImage>> GetByCompanyIdAsync(string companyId)
    {
        return await _context.CompanyImages
            .AsNoTracking()
            .Where(ci => ci.CompanyId == companyId)
            .ToListAsync();
    }
}
