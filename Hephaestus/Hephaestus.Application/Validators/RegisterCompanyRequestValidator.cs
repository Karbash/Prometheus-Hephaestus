using FluentValidation;
using Hephaestus.Domain.DTOs.Request;

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
            .MaximumLength(100).WithMessage("E-mail deve ter no máximo 100 caracteres.");

        RuleFor(x => x.PhoneNumber)
            .MaximumLength(20).When(x => x.PhoneNumber != null).WithMessage("Telefone deve ter no máximo 20 caracteres.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Senha é obrigatória.")
            .MinimumLength(8).WithMessage("Senha deve ter no mínimo 8 caracteres.");

        RuleFor(x => x.ApiKey)
            .NotEmpty().WithMessage("Chave API é obrigatória.")
            .MaximumLength(100).WithMessage("Chave API deve ter no máximo 100 caracteres.");

        RuleFor(x => x.FeeValue)
            .GreaterThanOrEqualTo(0).WithMessage("Valor da taxa deve ser maior ou igual a zero.");

        RuleFor(x => x.State)
            .NotEmpty().WithMessage("Estado é obrigatório.")
            .MaximumLength(50).WithMessage("Estado deve ter no máximo 50 caracteres.");

        RuleFor(x => x.City)
            .MaximumLength(100).When(x => x.City != null).WithMessage("Cidade deve ter no máximo 100 caracteres.");

        RuleFor(x => x.Neighborhood)
            .MaximumLength(100).When(x => x.Neighborhood != null).WithMessage("Bairro deve ter no máximo 100 caracteres.");

        RuleFor(x => x.Street)
            .MaximumLength(200).When(x => x.Street != null).WithMessage("Rua deve ter no máximo 200 caracteres.");

        RuleFor(x => x.Number)
            .MaximumLength(20).When(x => x.Number != null).WithMessage("Número deve ter no máximo 20 caracteres.");

        RuleFor(x => x.Latitude)
            .InclusiveBetween(-90, 90).When(x => x.Latitude != null).WithMessage("Latitude deve estar entre -90 e 90 graus.");

        RuleFor(x => x.Longitude)
            .InclusiveBetween(-180, 180).When(x => x.Longitude != null).WithMessage("Longitude deve estar entre -180 e 180 graus.");

        RuleFor(x => x.Slogan)
            .MaximumLength(200).When(x => x.Slogan != null).WithMessage("Slogan deve ter no máximo 200 caracteres.");

        RuleFor(x => x.Description)
            .MaximumLength(500).When(x => x.Description != null).WithMessage("Descrição deve ter no máximo 500 caracteres.");
    }
}
