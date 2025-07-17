using FluentValidation;
using Hephaestus.Domain.DTOs.Request;

namespace Hephaestus.Application.Validators;

public class RegisterCompanyRequestValidator : AbstractValidator<RegisterCompanyRequest>
{
    public RegisterCompanyRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Nome � obrigat�rio.")
            .MaximumLength(100).WithMessage("Nome deve ter no m�ximo 100 caracteres.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("E-mail � obrigat�rio.")
            .EmailAddress().WithMessage("E-mail inv�lido.")
            .MaximumLength(100).WithMessage("E-mail deve ter no m�ximo 100 caracteres.");

        RuleFor(x => x.PhoneNumber)
            .MaximumLength(20).When(x => x.PhoneNumber != null).WithMessage("Telefone deve ter no m�ximo 20 caracteres.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Senha � obrigat�ria.")
            .MinimumLength(8).WithMessage("Senha deve ter no m�nimo 8 caracteres.");

        RuleFor(x => x.ApiKey)
            .NotEmpty().WithMessage("Chave API � obrigat�ria.")
            .MaximumLength(100).WithMessage("Chave API deve ter no m�ximo 100 caracteres.");

        RuleFor(x => x.FeeValue)
            .GreaterThanOrEqualTo(0).WithMessage("Valor da taxa deve ser maior ou igual a zero.");

        RuleFor(x => x.Address).NotNull().WithMessage("Endereço é obrigatório.");
        RuleFor(x => x.Address.Street)
            .NotEmpty().WithMessage("Rua é obrigatória.")
            .MaximumLength(200).WithMessage("Rua deve ter no máximo 200 caracteres.");
        RuleFor(x => x.Address.Number)
            .NotEmpty().WithMessage("Número é obrigatório.")
            .MaximumLength(20).WithMessage("Número deve ter no máximo 20 caracteres.");
        RuleFor(x => x.Address.City)
            .NotEmpty().WithMessage("Cidade é obrigatória.")
            .MaximumLength(100).WithMessage("Cidade deve ter no máximo 100 caracteres.");
        RuleFor(x => x.Address.State)
            .NotEmpty().WithMessage("Estado é obrigatório.")
            .MaximumLength(2).WithMessage("Estado deve ter no máximo 2 caracteres.");
        RuleFor(x => x.Address.ZipCode)
            .MaximumLength(20).When(x => !string.IsNullOrEmpty(x.Address.ZipCode)).WithMessage("CEP deve ter no máximo 20 caracteres.");

        RuleFor(x => x.Slogan)
            .MaximumLength(200).When(x => x.Slogan != null).WithMessage("Slogan deve ter no m�ximo 200 caracteres.");

        RuleFor(x => x.Description)
            .MaximumLength(500).When(x => x.Description != null).WithMessage("Descri��o deve ter no m�ximo 500 caracteres.");
    }
}
