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

        RuleFor(x => x.State)
            .NotEmpty()
            .WithMessage("Estado � obrigat�rio.")
            .MaximumLength(2)
            .WithMessage("Estado deve ter no m�ximo 2 caracteres.");

        RuleFor(x => x.City)
            .MaximumLength(100)
            .When(x => !string.IsNullOrEmpty(x.City))
            .WithMessage("Cidade deve ter no m�ximo 100 caracteres.");

        RuleFor(x => x.Neighborhood)
            .MaximumLength(100)
            .When(x => !string.IsNullOrEmpty(x.Neighborhood))
            .WithMessage("Bairro deve ter no m�ximo 100 caracteres.");

        RuleFor(x => x.Street)
            .MaximumLength(200)
            .When(x => !string.IsNullOrEmpty(x.Street))
            .WithMessage("Rua deve ter no m�ximo 200 caracteres.");

        RuleFor(x => x.Number)
            .MaximumLength(20)
            .When(x => !string.IsNullOrEmpty(x.Number))
            .WithMessage("N�mero deve ter no m�ximo 20 caracteres.");

        RuleFor(x => x.Latitude)
            .InclusiveBetween(-90, 90)
            .When(x => x.Latitude.HasValue)
            .WithMessage("Latitude deve estar entre -90 e 90.");

        RuleFor(x => x.Longitude)
            .InclusiveBetween(-180, 180)
            .When(x => x.Longitude.HasValue)
            .WithMessage("Longitude deve estar entre -180 e 180.");

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
