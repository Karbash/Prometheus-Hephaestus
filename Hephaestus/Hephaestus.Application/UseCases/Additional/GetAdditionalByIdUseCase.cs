using Hephaestus.Application.DTOs.Response;
using Hephaestus.Application.Interfaces.Additional;
using Hephaestus.Domain.Repositories;

namespace Hephaestus.Application.UseCases.Additional;

public class GetAdditionalByIdUseCase : IGetAdditionalByIdUseCase
{
    private readonly IAdditionalRepository _additionalRepository;

    public GetAdditionalByIdUseCase(IAdditionalRepository additionalRepository)
    {
        _additionalRepository = additionalRepository;
    }

    public async Task<AdditionalResponse> ExecuteAsync(string id, string tenantId)
    {
        var additional = await _additionalRepository.GetByIdAsync(id, tenantId);
        if (additional == null)
            throw new KeyNotFoundException("Adicional não encontrado.");

        return new AdditionalResponse
        {
            Id = additional.Id,
            TenantId = additional.TenantId,
            Name = additional.Name,
            Price = additional.Price
        };
    }
}