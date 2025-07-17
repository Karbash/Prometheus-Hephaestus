namespace Hephaestus.Application.Interfaces.Administration;

using Hephaestus.Domain.DTOs.Response;
using System;
using System.Threading.Tasks;

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