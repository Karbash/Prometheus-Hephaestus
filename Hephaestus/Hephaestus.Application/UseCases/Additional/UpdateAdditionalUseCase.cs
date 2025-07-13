using FluentValidation;
using Hephaestus.Application.DTOs.Request;
using Hephaestus.Application.Interfaces.Additional;
using Hephaestus.Domain.Repositories;

namespace Hephaestus.Application.UseCases.Additional;

public class UpdateAdditionalUseCase : IUpdateAdditionalUseCase
{
    private readonly IAdditionalRepository _additionalRepository;
    private readonly IValidator<UpdateAdditionalRequest> _validator;

    public UpdateAdditionalUseCase(
        IAdditionalRepository additionalRepository,
        IValidator<UpdateAdditionalRequest> validator)
    {
        _additionalRepository = additionalRepository;
        _validator = validator;
    }

    public async Task ExecuteAsync(string id, UpdateAdditionalRequest request, string tenantId)
    {
        var validationResult = await _validator.ValidateAsync(request);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        if (request.Id != id)
            throw new ArgumentException("ID no corpo da requisição deve corresponder ao ID na URL.");

        var additional = await _additionalRepository.GetByIdAsync(id, tenantId);
        if (additional == null)
            throw new KeyNotFoundException("Adicional não encontrado.");

        additional.Name = request.Name ?? additional.Name;
        additional.Price = request.Price ?? additional.Price;

        await _additionalRepository.UpdateAsync(additional);
    }
}