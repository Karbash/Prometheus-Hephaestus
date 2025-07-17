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

    public async Task<IEnumerable<Company>> GetCompaniesWithinRadiusAsync(double centerLat, double centerLon, double radiusKm, string? city = null, string? neighborhood = null, List<string>? tagIds = null, List<string>? categoryIds = null, decimal? maxPrice = null, bool? openNow = null, int? dayOfWeek = null, string? time = null, bool? promotionActiveNow = null, int? promotionDayOfWeek = null, string? promotionTime = null)
    {
        _logger.LogInformation("Buscando empresas dentro de {RadiusKm} km de ({CenterLat}, {CenterLon}){City}{Neighborhood}", radiusKm, centerLat, centerLon, city != null ? $" na cidade {city}" : string.Empty, neighborhood != null ? $" no bairro {neighborhood}" : string.Empty);
        try
        {
            const double earthRadius = 6371; // Raio da Terra em km
            var addresses = await _context.Addresses
                .Where(a => a.EntityType == "Company" && a.Latitude != 0 && a.Longitude != 0)
                .ToListAsync();

            if (!string.IsNullOrWhiteSpace(city))
                addresses = addresses.Where(a => a.City.ToLower() == city.ToLower()).ToList();
            if (!string.IsNullOrWhiteSpace(neighborhood))
                addresses = addresses.Where(a => a.Neighborhood.ToLower() == neighborhood.ToLower()).ToList();

            var filteredAddresses = addresses.Where(a =>
                earthRadius * 2 * Math.Asin(Math.Sqrt(
                    Math.Pow(Math.Sin((a.Latitude - centerLat) * Math.PI / 180 / 2), 2) +
                    Math.Cos(centerLat * Math.PI / 180) * Math.Cos(a.Latitude * Math.PI / 180) *
                    Math.Pow(Math.Sin((a.Longitude - centerLon) * Math.PI / 180 / 2), 2)
                )) <= radiusKm
            ).ToList();

            var companyIds = filteredAddresses.Select(a => a.EntityId).Distinct().ToList();
            var companiesQuery = _context.Companies
                .Include(c => c.MenuItems)
                    .ThenInclude(mi => mi.MenuItemTags)
                .Where(c => companyIds.Contains(c.Id));

            // Filtro por tags/categorias
            if ((tagIds != null && tagIds.Any()) || (categoryIds != null && categoryIds.Any()))
            {
                companiesQuery = companiesQuery.Where(c =>
                    c.MenuItems.Any(mi =>
                        ((tagIds == null || !tagIds.Any()) || mi.MenuItemTags.Any(mt => tagIds.Contains(mt.TagId))) &&
                        ((categoryIds == null || !categoryIds.Any()) || categoryIds.Contains(mi.CategoryId))
                    )
                );
            }

            // Filtro por preço máximo
            if (maxPrice.HasValue)
            {
                companiesQuery = companiesQuery.Where(c =>
                    c.MenuItems.Any(mi => mi.Price <= maxPrice.Value)
                );
            }

            var companies = await companiesQuery.ToListAsync();

            // Filtro por horário de funcionamento
            if (openNow == true || dayOfWeek.HasValue || !string.IsNullOrWhiteSpace(time))
            {
                var now = DateTime.UtcNow;
                int targetDay = dayOfWeek ?? (openNow == true ? (int)now.DayOfWeek : 0);
                TimeSpan targetTime;
                if (!string.IsNullOrWhiteSpace(time) && TimeSpan.TryParse(time, out var parsedTime))
                {
                    targetTime = parsedTime;
                }
                else if (openNow == true)
                {
                    targetTime = now.TimeOfDay;
                }
                else
                {
                    targetTime = new TimeSpan(12, 0, 0); // Meio-dia como padrão se não informado
                }

                // Corrigir comparação de dia da semana (oh.DayOfWeek pode ser string)
                var targetDayStr = targetDay.ToString();
                var operatingHours = await _context.CompanyOperatingHours
                    .Where(oh => oh.IsOpen && oh.DayOfWeek == targetDayStr)
                    .ToListAsync();

                var companyIdsOpen = operatingHours
                    .Where(oh =>
                        TimeSpan.TryParse(oh.OpenTime, out var open) &&
                        TimeSpan.TryParse(oh.CloseTime, out var close) &&
                        open <= targetTime && close > targetTime)
                    .Select(oh => oh.CompanyId)
                    .ToHashSet();

                companies = companies.Where(c => companyIdsOpen.Contains(c.Id)).ToList();
            }

            // Filtro por promoções ativas
            if (promotionActiveNow == true || promotionDayOfWeek.HasValue || !string.IsNullOrWhiteSpace(promotionTime))
            {
                var now = DateTime.UtcNow;
                int promoDay = promotionDayOfWeek ?? (promotionActiveNow == true ? (int)now.DayOfWeek : 0);
                TimeSpan promoTime;
                if (!string.IsNullOrWhiteSpace(promotionTime) && TimeSpan.TryParse(promotionTime, out var parsedPromoTime))
                {
                    promoTime = parsedPromoTime;
                }
                else if (promotionActiveNow == true)
                {
                    promoTime = now.TimeOfDay;
                }
                else
                {
                    promoTime = new TimeSpan(12, 0, 0); // Meio-dia como padrão se não informado
                }

                var promoDayStr = ((DayOfWeek)promoDay).ToString().Substring(0, 3); // Ex: "Mon", "Tue"

                // Buscar promoções relevantes do banco (filtro por ativo e dia)
                var promoList = await _context.Promotions
                    .Where(p => p.IsActive && companyIds.Contains(p.CompanyId) && p.MenuItemId != null && p.DaysOfWeek.Contains(promoDayStr))
                    .ToListAsync();

                // Filtrar promoções por horário em memória
                var validPromoCompanyIds = promoList
                    .Where(p => p.Hours.Split(',').Any(h => {
                        var parts = h.Split('-');
                        if (parts.Length == 2 && TimeSpan.TryParse(parts[0], out var start) && TimeSpan.TryParse(parts[1], out var end))
                        {
                            return start <= promoTime && end > promoTime;
                        }
                        return false;
                    }))
                    .Select(p => p.CompanyId)
                    .ToHashSet();

                companies = companies.Where(c => validPromoCompanyIds.Contains(c.Id)).ToList();
            }

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
