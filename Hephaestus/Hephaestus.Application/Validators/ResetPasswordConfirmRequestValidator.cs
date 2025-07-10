using FluentValidation;
using Hephaestus.Application.DTOs.Request;

namespace Hephaestus.Application.Validators;

public class ResetPasswordConfirmRequestValidator : AbstractValidator<ResetPasswordConfirmRequest>
{
    public ResetPasswordConfirmRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("E-mail é obrigatório.")
            .EmailAddress().WithMessage("E-mail inválido.");

        RuleFor(x => x.ResetToken)
            .NotEmpty().WithMessage("Token de redefinição é obrigatório.");

        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage("Nova senha é obrigatória.")
            .MinimumLength(6).WithMessage("Nova senha deve ter pelo menos 6 caracteres.");
    }
}