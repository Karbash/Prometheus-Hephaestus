using FluentValidation;
using Hephaestus.Application.DTOs.Request;

namespace Hephaestus.Application.Validators;

public class CreateMenuItemRequestValidator : AbstractValidator<CreateMenuItemRequest>
{
    public CreateMenuItemRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Nome é obrigatório.")
            .MaximumLength(100).WithMessage("Nome deve ter no máximo 100 caracteres.");

        RuleFor(x => x.Description)
            .MaximumLength(500).When(x => x.Description != null).WithMessage("Descrição deve ter no máximo 500 caracteres.");

        RuleFor(x => x.CategoryId)
            .NotEmpty().WithMessage("CategoryId é obrigatório.")
            .Must(BeValidGuid).WithMessage("CategoryId deve ser um GUID válido.");

        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("Preço deve ser maior que zero.");

        RuleForEach(x => x.TagIds)
            .Must(BeValidGuid).WithMessage("Cada TagId deve ser um GUID válido.");

        RuleForEach(x => x.AvailableAdditionalIds)
            .Must(BeValidGuid).WithMessage("Cada AdditionalId deve ser um GUID válido.");
    }

    private bool BeValidGuid(string id)
    {
        return Guid.TryParse(id, out _);
    }
}