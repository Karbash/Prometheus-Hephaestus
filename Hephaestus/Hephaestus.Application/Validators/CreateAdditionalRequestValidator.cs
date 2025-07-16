using FluentValidation;
using Hephaestus.Domain.DTOs.Request;

namespace Hephaestus.Application.Validators;

public class CreateAdditionalRequestValidator : AbstractValidator<CreateAdditionalRequest>
{
    public CreateAdditionalRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Nome � obrigat�rio.")
            .MaximumLength(100).WithMessage("Nome deve ter no m�ximo 100 caracteres.");

        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("Pre�o deve ser maior que zero.");
    }
}
