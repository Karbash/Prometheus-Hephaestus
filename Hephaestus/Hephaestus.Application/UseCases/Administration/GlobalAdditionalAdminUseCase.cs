using Hephaestus.Application.Base;
using Hephaestus.Application.Interfaces.Administration;
using Hephaestus.Application.Services;
using Hephaestus.Domain.DTOs.Response;
using Hephaestus.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace Hephaestus.Application.UseCases.Administration;

public class GlobalAdditionalAdminUseCase : BaseUseCase, IGlobalAdditionalAdminUseCase
{
    private readonly IAdditionalRepository _additionalRepository;

    public GlobalAdditionalAdminUseCase(
        IAdditionalRepository additionalRepository,
        ILogger<GlobalAdditionalAdminUseCase> logger,
        IExceptionHandlerService exceptionHandler)
        : base(logger, exceptionHandler)
    {
        _additionalRepository = additionalRepository;
    }

    public async Task<PagedResult<AdditionalResponse>> ExecuteAsync(
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
        string? sortOrder = "asc")
    {
        return await ExecuteWithExceptionHandlingAsync(async () =>
        {
            var pagedAdditionals = await _additionalRepository.GetAllGlobalAsync(
                tenantId,
                name,
                isAvailable,
                precoMin,
                precoMax,
                dataInicial,
                dataFinal,
                pageNumber,
                pageSize,
                sortBy,
                sortOrder);

            var additionalResponses = pagedAdditionals.Items.Select(a => new AdditionalResponse
            {
                Id = a.Id,
                TenantId = a.TenantId,
                Name = a.Name,
                Price = a.Price,
                Description = a.Description,
                IsAvailable = a.IsAvailable,
                CreatedAt = a.CreatedAt,
                UpdatedAt = a.UpdatedAt,
                CreatedBy = a.CreatedBy,
                UpdatedBy = a.UpdatedBy
            }).ToList();

            return new PagedResult<AdditionalResponse>
            {
                Items = additionalResponses,
                TotalCount = pagedAdditionals.TotalCount,
                PageNumber = pagedAdditionals.PageNumber,
                PageSize = pagedAdditionals.PageSize
            };
        }, "GlobalAdditionalAdmin");
    }
} 