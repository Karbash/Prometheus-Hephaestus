namespace Hephaestus.Application.Interfaces.Administration;

using Hephaestus.Domain.DTOs.Response;
using System;
using System.Threading.Tasks;

public interface IGlobalAdditionalAdminUseCase
{
    Task<PagedResult<AdditionalResponse>> ExecuteAsync(
        string? tenantId = null,
        string? name = null,
        bool? isAvailable = null,
        decimal? precoMin = null,
        decimal? precoMax = null,
        DateTime? dataInicial = null,
        DateTime? dataFinal = null,
        int pageNumber = 1,
        int pageSize = 20,
        string? sortBy = null,
        string? sortOrder = "asc");
} 