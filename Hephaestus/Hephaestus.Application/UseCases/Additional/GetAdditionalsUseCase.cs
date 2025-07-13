using Hephaestus.Application.DTOs.Response;
using Hephaestus.Application.Interfaces.Additional;
using Hephaestus.Domain.Repositories;

namespace Hephaestus.Application.UseCases.Additional;

public class GetAdditionalsUseCase : IGetAdditionalsUseCase
{
    private readonly IAdditionalRepository _additionalRepository;

    public GetAdditionalsUseCase(IAdditionalRepository additionalRepository)
    {
        _additionalRepository = additionalRepository;
    }

    public async Task<IEnumerable<AdditionalResponse>> ExecuteAsync(string tenantId)
    {
        var additionals = await _additionalRepository.GetByTenantIdAsync(tenantId);
        return additionals.Select(a => new AdditionalResponse
        {
            Id = a.Id,
            TenantId = a.TenantId,
            Name = a.Name,
            Price = a.Price
        }).ToList();
    }
}