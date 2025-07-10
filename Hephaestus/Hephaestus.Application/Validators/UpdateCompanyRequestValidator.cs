using FluentValidation;
using Hephaestus.Application.DTOs.Request;

namespace Hephaestus.Application.Validators;

public class UpdateCompanyRequestValidator : AbstractValidator<CompanyRequest>
{
    public UpdateCompanyRequestValidator()
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

        RuleFor(x => x.ApiKey)
            .NotEmpty().WithMessage("API Key é obrigatória.")
            .MaximumLength(100).WithMessage("API Key deve ter no máximo 100 caracteres.");

        RuleFor(x => x.FeeType)
            .IsInEnum().WithMessage("Tipo de taxa inválido. Deve ser 'Percentage' ou 'Fixed'.");

        RuleFor(x => x.FeeValue)
            .GreaterThan(0).WithMessage("Valor da taxa deve ser maior que zero.");
    }
}
