using Hephaestus.Application.Base;
using Hephaestus.Application.Interfaces.Administration;
using Hephaestus.Application.Services;
using Hephaestus.Domain.DTOs.Response;
using Hephaestus.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace Hephaestus.Application.UseCases.Administration;

public class GlobalAddressAdminUseCase : BaseUseCase, IGlobalAddressAdminUseCase
{
    private readonly IAddressRepository _addressRepository;

    public GlobalAddressAdminUseCase(
        IAddressRepository addressRepository,
        ILogger<GlobalAddressAdminUseCase> logger,
        IExceptionHandlerService exceptionHandler)
        : base(logger, exceptionHandler)
    {
        _addressRepository = addressRepository;
    }

    public async Task<PagedResult<AddressResponse>> ExecuteAsync(
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
        string? sortOrder = "asc")
    {
        return await ExecuteWithExceptionHandlingAsync(async () =>
        {
            var pagedAddresses = await _addressRepository.GetAllGlobalAsync(
                entityId,
                entityType,
                city,
                state,
                type,
                dataInicial,
                dataFinal,
                pageNumber,
                pageSize,
                sortBy,
                sortOrder);

            var addressResponses = pagedAddresses.Items.Select(a => new AddressResponse
            {
                Id = a.Id,
                EntityId = a.EntityId,
                EntityType = a.EntityType,
                Street = a.Street,
                Number = a.Number,
                Complement = a.Complement,
                Neighborhood = a.Neighborhood,
                City = a.City,
                State = a.State,
                ZipCode = a.ZipCode,
                Country = a.Country,
                Latitude = a.Latitude,
                Longitude = a.Longitude,
                Type = a.Type,
                Reference = a.Reference,
                Notes = a.Notes,
                CreatedAt = a.CreatedAt,
                UpdatedAt = a.UpdatedAt
            }).ToList();

            return new PagedResult<AddressResponse>
            {
                Items = addressResponses,
                TotalCount = pagedAddresses.TotalCount,
                PageNumber = pagedAddresses.PageNumber,
                PageSize = pagedAddresses.PageSize
            };
        }, "GlobalAddressAdmin");
    }
} 