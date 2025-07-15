using Hephaestus.Domain.DTOs.Response;
using System.Security.Claims;

namespace Hephaestus.Application.Interfaces.Customer;

public interface IGetCustomerUseCase
{
    Task<PagedResult<CustomerResponse>> ExecuteAsync(string? phoneNumber, ClaimsPrincipal user, int pageNumber = 1, int pageSize = 20, string? sortBy = null, string? sortOrder = "asc");
}