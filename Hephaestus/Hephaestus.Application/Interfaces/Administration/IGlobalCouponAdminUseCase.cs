namespace Hephaestus.Application.Interfaces.Administration;

using Hephaestus.Domain.DTOs.Response;
using System.Threading.Tasks;

public interface IGlobalCouponAdminUseCase
{
    Task<PagedResult<CouponResponse>> ExecuteAsync(
        string? companyId = null,
        string? code = null,
        string? customerPhoneNumber = null,
        bool? isActive = null,
        int pageNumber = 1,
        int pageSize = 20,
        string? sortBy = null,
        string? sortOrder = "asc");
} 