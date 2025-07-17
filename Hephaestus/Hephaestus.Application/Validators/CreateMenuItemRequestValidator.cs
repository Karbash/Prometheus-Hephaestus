using FluentValidation;
using Hephaestus.Domain.DTOs.Request;

namespace Hephaestus.Application.Validators;

public class CreateMenuItemRequestValidator : AbstractValidator<CreateMenuItemRequest>
{
    public CreateMenuItemRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Nome � obrigat�rio.")
            .MaximumLength(100).WithMessage("Nome deve ter no m�ximo 100 caracteres.");

        RuleFor(x => x.Description)
            .MaximumLength(500).When(x => x.Description != null).WithMessage("Descri��o deve ter no m�ximo 500 caracteres.");

        RuleFor(x => x.CategoryId)
            .NotEmpty().WithMessage("CategoryId � obrigat�rio.")
            .Must(BeValidGuid).WithMessage("CategoryId deve ser um GUID v�lido.");

        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("Pre�o deve ser maior que zero.");

        RuleForEach(x => x.TagIds)
            .Must(BeValidGuid).WithMessage("Cada TagId deve ser um GUID v�lido.");
    }

    private bool BeValidGuid(string? id)
    {
        return id != null && Guid.TryParse(id, out _);
    }
}
