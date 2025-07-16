using FluentValidation;
using Hephaestus.Domain.DTOs.Request;

namespace Hephaestus.Application.Validators;

public class MfaRequestValidator : AbstractValidator<MfaRequest>
{
    public MfaRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("E-mail é obrigatório.")
            .EmailAddress().WithMessage("E-mail inválido.");

        RuleFor(x => x.MfaCode)
            .NotEmpty().WithMessage("Código MFA é obrigatório.")
            .Length(6).WithMessage("Código MFA deve ter 6 caracteres.");
    }
}
