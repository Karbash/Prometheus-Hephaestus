using Hephaestus.Domain.Entities;
using Hephaestus.Domain.Repositories;
using Hephaestus.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Hephaestus.Infrastructure.Repositories;

public class CompanyOperatingHourRepository : ICompanyOperatingHourRepository
{
    private readonly HephaestusDbContext _context;

    public CompanyOperatingHourRepository(HephaestusDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<CompanyOperatingHour>> GetByCompanyIdAsync(string companyId)
    {
        return await _context.CompanyOperatingHours
            .AsNoTracking()
            .Where(oh => oh.CompanyId == companyId)
            .ToListAsync();
    }
}
