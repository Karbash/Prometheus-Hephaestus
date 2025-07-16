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

        RuleFor(x => x.State)
            .NotEmpty().WithMessage("Estado � obrigat�rio.")
            .MaximumLength(50).WithMessage("Estado deve ter no m�ximo 50 caracteres.");

        RuleFor(x => x.City)
            .MaximumLength(100).When(x => x.City != null).WithMessage("Cidade deve ter no m�ximo 100 caracteres.");

        RuleFor(x => x.Neighborhood)
            .MaximumLength(100).When(x => x.Neighborhood != null).WithMessage("Bairro deve ter no m�ximo 100 caracteres.");

        RuleFor(x => x.Street)
            .MaximumLength(200).When(x => x.Street != null).WithMessage("Rua deve ter no m�ximo 200 caracteres.");

        RuleFor(x => x.Number)
            .MaximumLength(20).When(x => x.Number != null).WithMessage("N�mero deve ter no m�ximo 20 caracteres.");

        RuleFor(x => x.Latitude)
            .InclusiveBetween(-90, 90).When(x => x.Latitude != null).WithMessage("Latitude deve estar entre -90 e 90 graus.");

        RuleFor(x => x.Longitude)
            .InclusiveBetween(-180, 180).When(x => x.Longitude != null).WithMessage("Longitude deve estar entre -180 e 180 graus.");

        RuleFor(x => x.Slogan)
            .MaximumLength(200).When(x => x.Slogan != null).WithMessage("Slogan deve ter no m�ximo 200 caracteres.");

        RuleFor(x => x.Description)
            .MaximumLength(500).When(x => x.Description != null).WithMessage("Descri��o deve ter no m�ximo 500 caracteres.");
    }
}
