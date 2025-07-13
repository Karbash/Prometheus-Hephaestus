using FluentValidation;
using Hephaestus.Application.DTOs.Request;
using Hephaestus.Application.Interfaces.Additional;
using Hephaestus.Domain.Repositories;

namespace Hephaestus.Application.UseCases.Additional;

public class CreateAdditionalUseCase : ICreateAdditionalUseCase
{
    private readonly IAdditionalRepository _additionalRepository;
    private readonly IValidator<CreateAdditionalRequest> _validator;

    public CreateAdditionalUseCase(
        IAdditionalRepository additionalRepository,
        IValidator<CreateAdditionalRequest> validator)
    {
        _additionalRepository = additionalRepository;
        _validator = validator;
    }

    public async Task<string> ExecuteAsync(CreateAdditionalRequest request, string tenantId)
    {
        var validationResult = await _validator.ValidateAsync(request);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var additional = new Domain.Entities.Additional
        {
            Id = Guid.NewGuid().ToString(),
            TenantId = tenantId,
            Name = request.Name,
            Price = request.Price
        };

        await _additionalRepository.AddAsync(additional);
        return additional.Id;
    }
}