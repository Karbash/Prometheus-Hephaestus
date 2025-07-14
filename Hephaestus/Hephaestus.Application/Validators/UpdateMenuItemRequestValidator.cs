using FluentValidation;
using Hephaestus.Application.DTOs.Request;

namespace Hephaestus.Application.Validators;

public class UpdateMenuItemRequestValidator : AbstractValidator<UpdateMenuItemRequest>
{
    public UpdateMenuItemRequestValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Id é obrigatório.")
            .Must(BeValidGuid).WithMessage("Id deve ser um GUID válido.");

        RuleFor(x => x.Name)
            .NotEmpty().When(x => x.Name != null).WithMessage("Nome não pode ser vazio.")
            .MaximumLength(100).When(x => x.Name != null).WithMessage("Nome deve ter no máximo 100 caracteres.");

        RuleFor(x => x.Description)
            .MaximumLength(500).When(x => x.Description != null).WithMessage("Descrição deve ter no máximo 500 caracteres.");

        RuleFor(x => x.CategoryId)
            .Must(BeValidGuid).When(x => x.CategoryId != null).WithMessage("CategoryId deve ser um GUID válido.");

        RuleFor(x => x.Price)
            .GreaterThan(0).When(x => x.Price.HasValue).WithMessage("Preço deve ser maior que zero.");

        RuleForEach(x => x.TagIds)
            .Must(BeValidGuid).When(x => x.TagIds != null).WithMessage("Cada TagId deve ser um GUID válido.");

        RuleForEach(x => x.AvailableAdditionalIds)
            .Must(BeValidGuid).When(x => x.AvailableAdditionalIds != null).WithMessage("Cada AdditionalId deve ser um GUID válido.");
    }

    private bool BeValidGuid(string? id)
    {
        return id == null || Guid.TryParse(id, out _);
    }
}