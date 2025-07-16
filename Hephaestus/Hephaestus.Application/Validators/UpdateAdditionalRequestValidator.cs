using FluentValidation;
using Hephaestus.Domain.DTOs.Request;

namespace Hephaestus.Application.Validators;

public class UpdateAdditionalRequestValidator : AbstractValidator<UpdateAdditionalRequest>
{
    public UpdateAdditionalRequestValidator()
    {
        RuleFor(x => x.Name)
            .MaximumLength(100)
            .When(x => !string.IsNullOrEmpty(x.Name))
            .WithMessage("O nome deve ter no máximo 100 caracteres.");

        RuleFor(x => x.Price)
            .GreaterThanOrEqualTo(0)
            .When(x => x.Price.HasValue)
            .WithMessage("O preço deve ser maior ou igual a zero.");
    }
}
