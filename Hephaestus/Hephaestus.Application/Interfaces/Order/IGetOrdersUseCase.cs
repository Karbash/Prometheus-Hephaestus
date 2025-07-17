using Hephaestus.Domain.DTOs.Response;
using System.Security.Claims;

namespace Hephaestus.Application.Interfaces.Order;

public interface IGetOrdersUseCase
{
    Task<PagedResult<OrderResponse>> ExecuteAsync(System.Security.Claims.ClaimsPrincipal user, string? customerPhoneNumber, string? status, int pageNumber = 1, int pageSize = 20, string? sortBy = null, string? sortOrder = "asc");
}

public interface IGlobalOrderAdminUseCase
{
    Task<PagedResult<OrderResponse>> ExecuteAsync(
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
        string? sortOrder = "asc");
}

public interface IGlobalOrderItemAdminUseCase
{
    Task<PagedResult<OrderItemResponse>> ExecuteAsync(
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
        string? sortOrder = "asc");
}
