using FluentValidation;
using Hephaestus.Domain.DTOs.Request;

namespace Hephaestus.Application.Validators;

public class UpdateCompanyRequestValidator : AbstractValidator<UpdateCompanyRequest>
{
    public UpdateCompanyRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Nome � obrigat�rio.")
            .MaximumLength(100)
            .WithMessage("Nome deve ter no m�ximo 100 caracteres.");

        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("E-mail � obrigat�rio.")
            .EmailAddress()
            .WithMessage("E-mail deve ter um formato v�lido.");

        RuleFor(x => x.PhoneNumber)
            .MaximumLength(20)
            .When(x => !string.IsNullOrEmpty(x.PhoneNumber))
            .WithMessage("Telefone deve ter no m�ximo 20 caracteres.");

        RuleFor(x => x.ApiKey)
            .NotEmpty()
            .WithMessage("ApiKey � obrigat�ria.")
            .MaximumLength(100)
            .WithMessage("ApiKey deve ter no m�ximo 100 caracteres.");

        RuleFor(x => x.FeeType)
            .NotEmpty()
            .WithMessage("Tipo de taxa � obrigat�rio.")
            .IsInEnum()
            .WithMessage("Tipo de taxa inv�lido.");

        RuleFor(x => x.FeeValue)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Valor da taxa deve ser maior ou igual a zero.");

        When(x => x.Address != null, () =>
        {
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
        });

        RuleFor(x => x.Slogan)
            .MaximumLength(200)
            .When(x => !string.IsNullOrEmpty(x.Slogan))
            .WithMessage("Slogan deve ter no m�ximo 200 caracteres.");

        RuleFor(x => x.Description)
            .MaximumLength(1000)
            .When(x => !string.IsNullOrEmpty(x.Description))
            .WithMessage("Descri��o deve ter no m�ximo 1000 caracteres.");
    }
}
