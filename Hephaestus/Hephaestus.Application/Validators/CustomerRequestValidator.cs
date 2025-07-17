using FluentValidation;
using Hephaestus.Domain.DTOs.Request;

namespace Hephaestus.Application.Validators;

public class CustomerRequestValidator : AbstractValidator<CustomerRequest>
{
    public CustomerRequestValidator()
    {
        RuleFor(x => x.PhoneNumber)
            .NotEmpty().WithMessage("N�mero de telefone � obrigat�rio.")
            .MaximumLength(15).WithMessage("N�mero de telefone deve ter no m�ximo 15 caracteres.");

        RuleFor(x => x.Name)
            .MaximumLength(100).When(x => x.Name != null).WithMessage("Nome deve ter no m�ximo 100 caracteres.");

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
    }
}
