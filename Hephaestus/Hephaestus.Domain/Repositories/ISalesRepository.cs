using Hephaestus.Domain.Entities;

namespace Hephaestus.Domain.Repositories;

public interface ISalesRepository
{
    Task<IEnumerable<SalesLog>> GetSalesAsync(DateTime? startDate, DateTime? endDate, string? tenantId);
}