using FluentValidation;
using Hephaestus.Application.DTOs.Request;

namespace Hephaestus.Application.Validators;

public class RegisterCompanyRequestValidator : AbstractValidator<RegisterCompanyRequest>
{
    public RegisterCompanyRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Nome é obrigatório.")
            .MaximumLength(100).WithMessage("Nome deve ter no máximo 100 caracteres.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("E-mail é obrigatório.")
            .EmailAddress().WithMessage("E-mail inválido.")
            .MaximumLength(255).WithMessage("E-mail deve ter no máximo 255 caracteres.");

        RuleFor(x => x.PhoneNumber)
            .NotEmpty().WithMessage("Telefone é obrigatório.")
            .MaximumLength(15).WithMessage("Telefone deve ter no máximo 15 caracteres.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Senha é obrigatória.")
            .MinimumLength(6).WithMessage("Senha deve ter pelo menos 6 caracteres.");

        RuleFor(x => x.ApiKey)
            .NotEmpty().WithMessage("Chave API é obrigatória.")
            .MaximumLength(100).WithMessage("Chave API deve ter no máximo 100 caracteres.");

        RuleFor(x => x.FeeType)
            .IsInEnum().WithMessage("Tipo de taxa deve ser 'Percentage' ou 'Fixed'.");

        RuleFor(x => x.FeeValue)
            .GreaterThan(0).WithMessage("Valor da taxa deve ser maior que zero.");
    }
}