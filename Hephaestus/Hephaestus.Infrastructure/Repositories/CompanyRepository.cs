using Hephaestus.Domain.Entities;
using Hephaestus.Domain.Repositories;
using Hephaestus.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Hephaestus.Infrastructure.Repositories;

public class CompanyRepository : ICompanyRepository
{
    private readonly HephaestusDbContext _context;

    public CompanyRepository(HephaestusDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Company>> GetAllAsync(bool? isEnabled)
    {
        Console.WriteLine($"Buscando empresas com isEnabled: {isEnabled}");
        var query = _context.Companies.AsQueryable();
        if (isEnabled.HasValue)
            query = query.Where(c => c.IsEnabled == isEnabled.Value);
        var companies = await query.ToListAsync();
        Console.WriteLine($"Empresas encontradas: {JsonSerializer.Serialize(companies)}");
        return companies;
    }

    public async Task<Company?> GetByIdAsync(string id)
    {
        Console.WriteLine($"Buscando empresa por ID: {id}");
        var company = await _context.Companies.FindAsync(id);
        Console.WriteLine($"Empresa encontrada: {JsonSerializer.Serialize(company)}");
        return company;
    }

    public async Task<Company?> GetByEmailAsync(string email)
    {
        Console.WriteLine($"Buscando empresa por e-mail: {email}");
        var company = await _context.Companies.FirstOrDefaultAsync(c => c.Email == email);
        Console.WriteLine($"Empresa encontrada: {JsonSerializer.Serialize(company)}");
        return company;
    }

    public async Task<Company?> GetByPhoneNumberAsync(string phoneNumber)
    {
        Console.WriteLine($"Buscando empresa por telefone: {phoneNumber}");
        var company = await _context.Companies.FirstOrDefaultAsync(c => c.PhoneNumber == phoneNumber);
        Console.WriteLine($"Empresa encontrada: {JsonSerializer.Serialize(company)}");
        return company;
    }

    public async Task AddAsync(Company company)
    {
        Console.WriteLine($"Adicionando empresa: {JsonSerializer.Serialize(company)}");
        try
        {
            _context.Companies.Add(company);
            Console.WriteLine($"Estado da entidade antes de salvar: {_context.Entry(company).State}");
            var changes = await _context.SaveChangesAsync();
            Console.WriteLine($"Alterações salvas: {changes}");
            if (changes == 0)
                Console.WriteLine("Nenhuma alteração foi salva no banco de dados.");
            else
                Console.WriteLine("Empresa salva com sucesso.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao salvar empresa: {ex.Message}");
            Console.WriteLine($"StackTrace: {ex.StackTrace}");
            throw;
        }
    }

    public async Task UpdateAsync(Company company)
    {
        Console.WriteLine($"Atualizando empresa: {JsonSerializer.Serialize(company)}");
        try
        {
            _context.Companies.Update(company);
            Console.WriteLine($"Estado da entidade antes de salvar: {_context.Entry(company).State}");
            var changes = await _context.SaveChangesAsync();
            Console.WriteLine($"Alterações salvas: {changes}");
            if (changes == 0)
                Console.WriteLine("Nenhuma alteração foi salva no banco de dados.");
            else
                Console.WriteLine("Empresa atualizada com sucesso.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao atualizar empresa: {ex.Message}");
            Console.WriteLine($"StackTrace: {ex.StackTrace}");
            throw;
        }
    }

    public async Task<IEnumerable<Company>> GetCompaniesWithinRadiusAsync(double centerLat, double centerLon, double radiusKm, string? city = null, string? neighborhood = null)
    {
        Console.WriteLine($"Buscando empresas dentro de {radiusKm} km de ({centerLat}, {centerLon})" +
                         (city != null ? $" na cidade {city}" : "") +
                         (neighborhood != null ? $" no bairro {neighborhood}" : ""));
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

            Console.WriteLine($"Empresas encontradas no raio: {JsonSerializer.Serialize(companies)}");
            return companies;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao buscar empresas no raio: {ex.Message}");
            Console.WriteLine($"StackTrace: {ex.StackTrace}");
            throw;
        }
    }
}