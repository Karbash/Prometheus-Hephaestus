using FluentValidation;
using Hephaestus.Domain.DTOs.Request;

namespace Hephaestus.Application.Validators;

public class ResetPasswordConfirmRequestValidator : AbstractValidator<ResetPasswordConfirmRequest>
{
    public ResetPasswordConfirmRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("E-mail � obrigat�rio.")
            .EmailAddress().WithMessage("E-mail inv�lido.");

        RuleFor(x => x.ResetToken)
            .NotEmpty().WithMessage("Token de redefini��o � obrigat�rio.");

        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage("Nova senha � obrigat�ria.")
            .MinimumLength(6).WithMessage("Nova senha deve ter pelo menos 6 caracteres.");
    }
}
