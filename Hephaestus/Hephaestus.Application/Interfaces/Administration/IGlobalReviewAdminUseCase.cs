namespace Hephaestus.Application.Interfaces.Administration;

using Hephaestus.Domain.DTOs.Response;
using System;
using System.Threading.Tasks;

public interface IGlobalReviewAdminUseCase
{
    Task<PagedResult<ReviewResponse>> ExecuteAsync(
        string? companyId = null,
        string? customerId = null,
        string? orderId = null,
        int? ratingMin = null,
        int? ratingMax = null,
        bool? isActive = null,
        DateTime? dataInicial = null,
        DateTime? dataFinal = null,
        int pageNumber = 1,
        int pageSize = 20,
        string? sortBy = null,
        string? sortOrder = "asc");
} 