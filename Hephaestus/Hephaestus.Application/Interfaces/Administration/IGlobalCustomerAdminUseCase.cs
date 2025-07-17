namespace Hephaestus.Application.Interfaces.Administration;

using Hephaestus.Domain.DTOs.Response;
using System.Threading.Tasks;

public interface IGlobalCustomerAdminUseCase
{
    Task<PagedResult<CustomerResponse>> ExecuteAsync(
        string? companyId = null,
        string? name = null,
        string? phoneNumber = null,
        bool? isActive = null,
        int pageNumber = 1,
        int pageSize = 20,
        string? sortBy = null,
        string? sortOrder = "asc");
} 