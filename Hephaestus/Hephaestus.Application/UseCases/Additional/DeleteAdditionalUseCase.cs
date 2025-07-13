using Hephaestus.Application.Interfaces.Additional;
using Hephaestus.Domain.Repositories;

namespace Hephaestus.Application.UseCases.Additional;

public class DeleteAdditionalUseCase : IDeleteAdditionalUseCase
{
    private readonly IAdditionalRepository _additionalRepository;

    public DeleteAdditionalUseCase(IAdditionalRepository additionalRepository)
    {
        _additionalRepository = additionalRepository;
    }

    public async Task ExecuteAsync(string id, string tenantId)
    {
        await _additionalRepository.DeleteAsync(id, tenantId);
    }
}