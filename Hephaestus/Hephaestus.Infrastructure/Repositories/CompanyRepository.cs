using Hephaestus.Domain.DTOs.Response;
using Hephaestus.Domain.Entities;
using Hephaestus.Domain.Repositories;
using Hephaestus.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Hephaestus.Infrastructure.Repositories;

public class CompanyRepository : ICompanyRepository
{
    private readonly HephaestusDbContext _context;
    private readonly ILogger<CompanyRepository> _logger;

    public CompanyRepository(HephaestusDbContext context, ILogger<CompanyRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<PagedResult<Company>> GetAllAsync(bool? isEnabled, int pageNumber = 1, int pageSize = 20)
    {
        _logger.LogInformation("Buscando empresas com isEnabled: {IsEnabled}", isEnabled);
        var query = _context.Companies.AsNoTracking().AsQueryable();
        if (isEnabled.HasValue)
            query = query.Where(c => c.IsEnabled == isEnabled.Value);
        var totalCount = await query.CountAsync();
        var items = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();
        return new PagedResult<Company>
        {
            Items = items,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public async Task<Company?> GetByIdAsync(string id)
    {
        _logger.LogInformation("Buscando empresa por ID: {Id}", id);
        var company = await _context.Companies.FindAsync(id);
        _logger.LogInformation("Empresa encontrada: {@Company}", company);
        return company;
    }

    public async Task<Company?> GetByEmailAsync(string email)
    {
        _logger.LogInformation("Buscando empresa por e-mail: {Email}", email);
        var company = await _context.Companies.FirstOrDefaultAsync(c => c.Email == email);
        _logger.LogInformation("Empresa encontrada: {@Company}", company);
        return company;
    }

    public async Task<Company?> GetByPhoneNumberAsync(string phoneNumber)
    {
        _logger.LogInformation("Buscando empresa por telefone: {PhoneNumber}", phoneNumber);
        var company = await _context.Companies.FirstOrDefaultAsync(c => c.PhoneNumber == phoneNumber);
        _logger.LogInformation("Empresa encontrada: {@Company}", company);
        return company;
    }

    public async Task AddAsync(Company company)
    {
        _logger.LogInformation("Adicionando empresa: {@Company}", company);
        try
        {
            _context.Companies.Add(company);
            _logger.LogDebug("Estado da entidade antes de salvar: {State}", _context.Entry(company).State);
            var changes = await _context.SaveChangesAsync();
            _logger.LogInformation("Alterações salvas: {Changes}", changes);
            if (changes == 0)
                _logger.LogWarning("Nenhuma alteração foi salva no banco de dados.");
            else
                _logger.LogInformation("Empresa salva com sucesso.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao salvar empresa: {Message}", ex.Message);
            throw;
        }
    }

    public async Task UpdateAsync(Company company)
    {
        _logger.LogInformation("Atualizando empresa: {@Company}", company);
        try
        {
            _context.Companies.Update(company);
            _logger.LogDebug("Estado da entidade antes de salvar: {State}", _context.Entry(company).State);
            var changes = await _context.SaveChangesAsync();
            _logger.LogInformation("Alterações salvas: {Changes}", changes);
            if (changes == 0)
                _logger.LogWarning("Nenhuma alteração foi salva no banco de dados.");
            else
                _logger.LogInformation("Empresa atualizada com sucesso.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar empresa: {Message}", ex.Message);
            throw;
        }
    }

    public async Task<IEnumerable<Company>> GetCompaniesWithinRadiusAsync(double centerLat, double centerLon, double radiusKm, string? city = null, string? neighborhood = null)
    {
        _logger.LogInformation("Buscando empresas dentro de {RadiusKm} km de ({CenterLat}, {CenterLon}){City}{Neighborhood}", radiusKm, centerLat, centerLon, city != null ? $" na cidade {city}" : string.Empty, neighborhood != null ? $" no bairro {neighborhood}" : string.Empty);
        try
        {
            const double earthRadius = 6371; // Raio da Terra em km
            var query = _context.Companies
                .Where(c => c.Latitude != null && c.Longitude != null);

            if (!string.IsNullOrWhiteSpace(city))
                query = query.Where(c => c.City != null && c.City.ToLower() == city.ToLower());

            if (!string.IsNullOrWhiteSpace(neighborhood))
                query = query.Where(c => c.Neighborhood != null && c.Neighborhood.ToLower() == neighborhood.ToLower());

            var companies = await query
                .Where(c => earthRadius * 2 * Math.Asin(Math.Sqrt(
                    Math.Pow(Math.Sin((c.Latitude.Value - centerLat) * Math.PI / 180 / 2), 2) +
                    Math.Cos(centerLat * Math.PI / 180) * Math.Cos(c.Latitude.Value * Math.PI / 180) *
                    Math.Pow(Math.Sin((c.Longitude.Value - centerLon) * Math.PI / 180 / 2), 2)
                )) <= radiusKm)
                .ToListAsync();

            _logger.LogInformation("Empresas encontradas no raio: {@Companies}", companies);
            return companies;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar empresas no raio: {Message}", ex.Message);
            throw;
        }
    }
}
