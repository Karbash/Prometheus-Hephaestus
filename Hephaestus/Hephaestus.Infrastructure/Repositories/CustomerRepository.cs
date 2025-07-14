using Hephaestus.Domain.Entities;
using Hephaestus.Domain.Repositories;
using Hephaestus.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Hephaestus.Application.DTOs.Response;

namespace Hephaestus.Infrastructure.Repositories;

public class CustomerRepository : ICustomerRepository
{
    private readonly HephaestusDbContext _context;

    public CustomerRepository(HephaestusDbContext context)
    {
        _context = context;
    }

    public async Task<Customer?> GetByPhoneNumberAsync(string phoneNumber, string tenantId)
    {
        if (string.IsNullOrEmpty(phoneNumber) || string.IsNullOrEmpty(tenantId))
            throw new ArgumentException("PhoneNumber e TenantId são obrigatórios.");

        return await _context.Customers
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.PhoneNumber == phoneNumber && c.TenantId == tenantId);
    }

    public async Task<PagedResult<Customer>> GetAllAsync(string? phoneNumber, string tenantId, int pageNumber = 1, int pageSize = 20)
    {
        var query = _context.Customers.AsNoTracking().Where(c => c.TenantId == tenantId);
        if (!string.IsNullOrEmpty(phoneNumber))
            query = query.Where(c => c.PhoneNumber == phoneNumber);
        var totalCount = await query.CountAsync();
        var items = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();
        return new PagedResult<Customer>
        {
            Items = items,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public async Task<Customer?> GetByIdAsync(string id, string tenantId)
    {
        if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(tenantId))
            throw new ArgumentException("Id e TenantId são obrigatórios.");

        return await _context.Customers
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id && c.TenantId == tenantId);
    }

    public async Task AddAsync(Customer customer)
    {
        if (customer == null)
            throw new ArgumentNullException(nameof(customer));

        await _context.Customers.AddAsync(customer);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Customer customer)
    {
        if (customer == null)
            throw new ArgumentNullException(nameof(customer));

        var existingCustomer = await _context.Customers
            .FirstOrDefaultAsync(c => c.Id == customer.Id && c.TenantId == customer.TenantId);

        if (existingCustomer == null)
            return; // Não lança exceção, deixa o UseCase tratar

        _context.Entry(existingCustomer).CurrentValues.SetValues(customer);
        await _context.SaveChangesAsync();
    }
}