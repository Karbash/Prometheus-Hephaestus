using Hephaestus.Domain.Entities;
using Hephaestus.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Hephaestus.Infrastructure.Data;

public class CompanyRepository : ICompanyRepository
{
    private readonly HephaestusDbContext _context;

    public CompanyRepository(HephaestusDbContext context)
    {
        _context = context;
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
            {
                Console.WriteLine("Nenhuma alteração foi salva no banco de dados.");
            }
            else
            {
                Console.WriteLine("Empresa salva com sucesso.");
            }
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
            {
                Console.WriteLine("Nenhuma alteração foi salva no banco de dados.");
            }
            else
            {
                Console.WriteLine("Empresa atualizada com sucesso.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao atualizar empresa: {ex.Message}");
            Console.WriteLine($"StackTrace: {ex.StackTrace}");
            throw;
        }
    }
}