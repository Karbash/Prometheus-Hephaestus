namespace Hephaestus.Application.Interfaces.Administration;

using Hephaestus.Domain.DTOs.Response;
using System;
using System.Threading.Tasks;

public interface IGlobalAddressAdminUseCase
{
    Task<PagedResult<AddressResponse>> ExecuteAsync(
        string? entityId = null,
        string? entityType = null,
        string? city = null,
        string? state = null,
        string? type = null,
        DateTime? dataInicial = null,
        DateTime? dataFinal = null,
        int pageNumber = 1,
        int pageSize = 20,
        string? sortBy = null,
        string? sortOrder = "asc");
} 