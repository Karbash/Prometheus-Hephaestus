using FluentValidation;
using Hephaestus.Application.DTOs.Request;

namespace Hephaestus.Application.Validators;

public class UpdateCompanyRequestValidator : AbstractValidator<UpdateCompanyRequest>
{
    public UpdateCompanyRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Nome é obrigatório.")
            .MaximumLength(100)
            .WithMessage("Nome deve ter no máximo 100 caracteres.");

        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("E-mail é obrigatório.")
            .EmailAddress()
            .WithMessage("E-mail deve ter um formato válido.");

        RuleFor(x => x.PhoneNumber)
            .MaximumLength(20)
            .When(x => !string.IsNullOrEmpty(x.PhoneNumber))
            .WithMessage("Telefone deve ter no máximo 20 caracteres.");

        RuleFor(x => x.ApiKey)
            .NotEmpty()
            .WithMessage("ApiKey é obrigatória.")
            .MaximumLength(100)
            .WithMessage("ApiKey deve ter no máximo 100 caracteres.");

        RuleFor(x => x.FeeType)
            .NotEmpty()
            .WithMessage("Tipo de taxa é obrigatório.")
            .IsInEnum()
            .WithMessage("Tipo de taxa inválido.");

        RuleFor(x => x.FeeValue)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Valor da taxa deve ser maior ou igual a zero.");

        RuleFor(x => x.State)
            .NotEmpty()
            .WithMessage("Estado é obrigatório.")
            .MaximumLength(2)
            .WithMessage("Estado deve ter no máximo 2 caracteres.");

        RuleFor(x => x.City)
            .MaximumLength(100)
            .When(x => !string.IsNullOrEmpty(x.City))
            .WithMessage("Cidade deve ter no máximo 100 caracteres.");

        RuleFor(x => x.Neighborhood)
            .MaximumLength(100)
            .When(x => !string.IsNullOrEmpty(x.Neighborhood))
            .WithMessage("Bairro deve ter no máximo 100 caracteres.");

        RuleFor(x => x.Street)
            .MaximumLength(200)
            .When(x => !string.IsNullOrEmpty(x.Street))
            .WithMessage("Rua deve ter no máximo 200 caracteres.");

        RuleFor(x => x.Number)
            .MaximumLength(20)
            .When(x => !string.IsNullOrEmpty(x.Number))
            .WithMessage("Número deve ter no máximo 20 caracteres.");

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
            .WithMessage("Slogan deve ter no máximo 200 caracteres.");

        RuleFor(x => x.Description)
            .MaximumLength(1000)
            .When(x => !string.IsNullOrEmpty(x.Description))
            .WithMessage("Descrição deve ter no máximo 1000 caracteres.");
    }
}