using FluentValidation;
using Hephaestus.Domain.DTOs.Request;

namespace Hephaestus.Application.Validators;

public class TagRequestValidator : AbstractValidator<TagRequest>
{
    public TagRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Nome da tag � obrigat�rio.")
            .MaximumLength(50).WithMessage("Nome da tag deve ter no m�ximo 50 caracteres.");
    }
}
