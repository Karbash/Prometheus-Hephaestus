using FluentValidation;
using Hephaestus.Domain.DTOs.Request;

namespace Hephaestus.Application.Validators;

public class MfaRequestValidator : AbstractValidator<MfaRequest>
{
    public MfaRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("E-mail � obrigat�rio.")
            .EmailAddress().WithMessage("E-mail inv�lido.");

        RuleFor(x => x.MfaCode)
            .NotEmpty().WithMessage("C�digo MFA � obrigat�rio.")
            .Length(6).WithMessage("C�digo MFA deve ter 6 caracteres.");
    }
}
