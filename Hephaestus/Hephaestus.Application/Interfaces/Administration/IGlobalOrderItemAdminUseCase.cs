namespace Hephaestus.Application.Interfaces.Administration;

using Hephaestus.Domain.DTOs.Response;
using System;
using System.Threading.Tasks;

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