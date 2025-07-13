using FluentValidation;
using Hephaestus.Application.DTOs.Request;

namespace Hephaestus.Application.Validators;

public class UpdateAdditionalRequestValidator : AbstractValidator<UpdateAdditionalRequest>
{
    public UpdateAdditionalRequestValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Id é obrigatório.")
            .Must(BeValidGuid).WithMessage("Id deve ser um GUID válido.");

        RuleFor(x => x.Name)
            .NotEmpty().When(x => x.Name != null).WithMessage("Nome não pode ser vazio.")
            .MaximumLength(100).When(x => x.Name != null).WithMessage("Nome deve ter no máximo 100 caracteres.");

        RuleFor(x => x.Price)
            .GreaterThan(0).When(x => x.Price.HasValue).WithMessage("Preço deve ser maior que zero.");
    }

    private bool BeValidGuid(string id)
    {
        return id == null || Guid.TryParse(id, out _);
    }
}