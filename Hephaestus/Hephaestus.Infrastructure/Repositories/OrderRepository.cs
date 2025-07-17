using Hephaestus.Domain.DTOs.Response;
using Hephaestus.Domain.Entities;
using Hephaestus.Domain.Repositories;
using Hephaestus.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hephaestus.Infrastructure.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly HephaestusDbContext _context;

    public OrderRepository(HephaestusDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Order order)
    {
        await _context.Orders.AddAsync(order);
        await _context.SaveChangesAsync();
    }

    public async Task<Order?> GetByIdAsync(string id, string tenantId)
    {
        return await _context.Orders
            .Include(o => o.OrderItems)
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == id && o.TenantId == tenantId);
    }

    public async Task<Order?> GetByIdWithItemsAsync(string id, string tenantId)
    {
        return await _context.Orders
            .Include(o => o.OrderItems)
            .FirstOrDefaultAsync(o => o.Id == id && o.TenantId == tenantId);
    }

    public async Task<PagedResult<Order>> GetByTenantIdAsync(string tenantId, string? customerPhoneNumber, string? status, int pageNumber = 1, int pageSize = 20, string? sortBy = null, string? sortOrder = "asc")
    {
        var query = _context.Orders
            .Include(o => o.OrderItems)
            .AsNoTracking()
            .Where(o => o.TenantId == tenantId);

        if (!string.IsNullOrEmpty(customerPhoneNumber))
        {
            query = query.Where(o => o.CustomerPhoneNumber == customerPhoneNumber);
        }

        if (!string.IsNullOrEmpty(status))
        {
            query = query.Where(o => o.Status.ToString() == status);
        }

        // Ordena��o din�mica
        if (!string.IsNullOrEmpty(sortBy))
        {
            query = sortOrder?.ToLower() == "desc"
                ? query.OrderByDescending(e => EF.Property<object>(e, sortBy))
                : query.OrderBy(e => EF.Property<object>(e, sortBy));
        }
        else
        {
            query = query.OrderByDescending(o => o.CreatedAt);
        }

        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<Order>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    public async Task UpdateAsync(Order order)
    {
        // Carrega a Order do banco com tracking e seus itens
        var existingOrder = await _context.Orders
            .Include(o => o.OrderItems)
            .FirstOrDefaultAsync(o => o.Id == order.Id && o.TenantId == order.TenantId);
        if (existingOrder == null)
            return;

        // Atualiza campos simples
        existingOrder.CustomerPhoneNumber = order.CustomerPhoneNumber;
        existingOrder.PromotionId = order.PromotionId;
        existingOrder.CouponId = order.CouponId;
        existingOrder.Status = order.Status;
        existingOrder.PaymentStatus = order.PaymentStatus;
        existingOrder.UpdatedAt = order.UpdatedAt;
        existingOrder.TotalAmount = order.TotalAmount;
        existingOrder.PlatformFee = order.PlatformFee;

        // Merge seguro dos OrderItems
        var updatedItems = order.OrderItems;
        // Atualiza e adiciona itens
        foreach (var item in updatedItems)
        {
            var existingItem = existingOrder.OrderItems.FirstOrDefault(oi => oi.Id == item.Id);
            if (existingItem != null)
            {
                // Atualiza item existente
                existingItem.MenuItemId = item.MenuItemId;
                existingItem.Quantity = item.Quantity;
                existingItem.UnitPrice = item.UnitPrice;
                existingItem.Notes = item.Notes;
                existingItem.Customizations = item.Customizations;
            }
            else
            {
                // Novo item
                existingOrder.OrderItems.Add(item);
            }
        }
        // Remove itens que n�o est�o mais presentes
        var itemsToRemove = existingOrder.OrderItems.Where(oi => !updatedItems.Any(ui => ui.Id == oi.Id)).ToList();
        foreach (var item in itemsToRemove)
        {
            existingOrder.OrderItems.Remove(item);
        }

        await _context.SaveChangesAsync();
    }

    public async Task<List<Order>> GetPendingOrdersOlderThanAsync(DateTime cutoffUtc)
    {
        return await _context.Orders
            .Where(o => o.Status == Hephaestus.Domain.Enum.OrderStatus.Pending && o.CreatedAt < cutoffUtc)
            .ToListAsync();
    }

    public async Task DeleteAsync(string id, string tenantId)
    {
        var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == id && o.TenantId == tenantId);
        if (order != null)
        {
            _context.Orders.Remove(order);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<PagedResult<Order>> GetAllGlobalAsync(
        string? companyId = null,
        string? customerId = null,
        string? customerPhoneNumber = null,
        string? status = null,
        string? paymentStatus = null,
        DateTime? dataInicial = null,
        DateTime? dataFinal = null,
        decimal? valorMin = null,
        decimal? valorMax = null,
        int pageNumber = 1,
        int pageSize = 20,
        string? sortBy = null,
        string? sortOrder = "asc")
    {
        var query = _context.Orders
            .Include(o => o.OrderItems)
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrEmpty(companyId))
            query = query.Where(o => o.CompanyId == companyId);
        if (!string.IsNullOrEmpty(customerId))
            query = query.Where(o => o.CustomerId == customerId);
        if (!string.IsNullOrEmpty(customerPhoneNumber))
            query = query.Where(o => o.CustomerPhoneNumber == customerPhoneNumber);
        if (!string.IsNullOrEmpty(status))
            query = query.Where(o => o.Status.ToString() == status);
        if (!string.IsNullOrEmpty(paymentStatus))
            query = query.Where(o => o.PaymentStatus.ToString() == paymentStatus);
        if (dataInicial.HasValue)
            query = query.Where(o => o.CreatedAt >= dataInicial.Value);
        if (dataFinal.HasValue)
            query = query.Where(o => o.CreatedAt <= dataFinal.Value);
        if (valorMin.HasValue)
            query = query.Where(o => o.TotalAmount >= valorMin.Value);
        if (valorMax.HasValue)
            query = query.Where(o => o.TotalAmount <= valorMax.Value);

        // Ordenação dinâmica
        if (!string.IsNullOrEmpty(sortBy))
        {
            query = sortOrder?.ToLower() == "desc"
                ? query.OrderByDescending(e => EF.Property<object>(e, sortBy))
                : query.OrderBy(e => EF.Property<object>(e, sortBy));
        }
        else
        {
            query = query.OrderByDescending(o => o.CreatedAt);
        }

        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<Order>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    public async Task<PagedResult<OrderItem>> GetAllOrderItemsGlobalAsync(
        string? orderId = null,
        string? companyId = null,
        string? customerId = null,
        string? menuItemId = null,
        DateTime? dataInicial = null,
        DateTime? dataFinal = null,
        decimal? valorMin = null,
        decimal? valorMax = null,
        int pageNumber = 1,
        int pageSize = 20,
        string? sortBy = null,
        string? sortOrder = "asc")
    {
        var query = _context.OrderItems
            .Include(oi => oi.Order)
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrEmpty(orderId))
            query = query.Where(oi => oi.OrderId == orderId);
        if (!string.IsNullOrEmpty(companyId))
            query = query.Where(oi => oi.Order.CompanyId == companyId);
        if (!string.IsNullOrEmpty(customerId))
            query = query.Where(oi => oi.Order.CustomerId == customerId);
        if (!string.IsNullOrEmpty(menuItemId))
            query = query.Where(oi => oi.MenuItemId == menuItemId);
        if (dataInicial.HasValue)
            query = query.Where(oi => oi.CreatedAt >= dataInicial.Value);
        if (dataFinal.HasValue)
            query = query.Where(oi => oi.CreatedAt <= dataFinal.Value);
        if (valorMin.HasValue)
            query = query.Where(oi => oi.UnitPrice >= valorMin.Value);
        if (valorMax.HasValue)
            query = query.Where(oi => oi.UnitPrice <= valorMax.Value);

        // Ordenação dinâmica
        if (!string.IsNullOrEmpty(sortBy))
        {
            query = sortOrder?.ToLower() == "desc"
                ? query.OrderByDescending(e => EF.Property<object>(e, sortBy))
                : query.OrderBy(e => EF.Property<object>(e, sortBy));
        }
        else
        {
            query = query.OrderByDescending(oi => oi.CreatedAt);
        }

        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<OrderItem>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }
}
