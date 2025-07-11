using FluentValidation;
using Hephaestus.Application.DTOs.Request;

namespace Hephaestus.Application.Validators;

/// <summary>
/// Validador para o DTO <see cref="UpdateCompanyRequest"/> usado na atualização de empresas.
/// </summary>
public class UpdateCompanyRequestValidator : AbstractValidator<UpdateCompanyRequest>
{
    public UpdateCompanyRequestValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("ID é obrigatório.")
            .Must(BeValidGuid).WithMessage("ID deve ser um GUID válido.");

        RuleFor(x => x.Name)
            .MaximumLength(100).When(x => x.Name != null).WithMessage("Nome deve ter no máximo 100 caracteres.");

        RuleFor(x => x.Email)
            .EmailAddress().When(x => x.Email != null).WithMessage("E-mail inválido.")
            .MaximumLength(255).When(x => x.Email != null).WithMessage("E-mail deve ter no máximo 255 caracteres.");

        RuleFor(x => x.PhoneNumber)
            .MaximumLength(15).When(x => x.PhoneNumber != null).WithMessage("Telefone deve ter no máximo 15 caracteres.");

        RuleFor(x => x.ApiKey)
            .MaximumLength(100).When(x => x.ApiKey != null).WithMessage("API Key deve ter no máximo 100 caracteres.");

        RuleFor(x => x.FeeType)
            .IsInEnum().When(x => x.FeeType != null).WithMessage("Tipo de taxa inválido. Deve ser 'Percentage' ou 'Fixed'.");

        RuleFor(x => x.FeeValue)
            .GreaterThan(0).When(x => x.FeeValue != null).WithMessage("Valor da taxa deve ser maior que zero.");

        RuleFor(x => x.City)
            .MaximumLength(100).When(x => x.City != null).WithMessage("Cidade deve ter no máximo 100 caracteres.");

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

    private bool BeValidGuid(string id)
    {
        return Guid.TryParse(id, out _);
    }
}