using Hephaestus.Domain.DTOs.Response;
using System.Security.Claims;

namespace Hephaestus.Application.Interfaces.Order;

public interface IGetOrdersUseCase
{
    Task<PagedResult<OrderResponse>> ExecuteAsync(System.Security.Claims.ClaimsPrincipal user, string? customerPhoneNumber, string? status, int pageNumber = 1, int pageSize = 20, string? sortBy = null, string? sortOrder = "asc");
}