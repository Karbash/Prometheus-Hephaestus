using FluentValidation;
using Hephaestus.Domain.DTOs.Request;

namespace Hephaestus.Application.Validators;

public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("E-mail � obrigat�rio.")
            .EmailAddress().WithMessage("E-mail inv�lido.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Senha � obrigat�ria.")
            .MinimumLength(6).WithMessage("Senha deve ter pelo menos 6 caracteres.");
    }
}
