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

        RuleFor(x => x.State)
            .NotEmpty().WithMessage("Estado � obrigat�rio.")
            .MaximumLength(50).WithMessage("Estado deve ter no m�ximo 50 caracteres.");

        RuleFor(x => x.City)
            .MaximumLength(100).When(x => x.City != null).WithMessage("Cidade deve ter no m�ximo 100 caracteres.");

        RuleFor(x => x.Street)
            .MaximumLength(200).When(x => x.Street != null).WithMessage("Rua deve ter no m�ximo 200 caracteres.");

        RuleFor(x => x.Number)
            .MaximumLength(20).When(x => x.Number != null).WithMessage("N�mero deve ter no m�ximo 20 caracteres.");

        RuleFor(x => x.Latitude)
            .InclusiveBetween(-90, 90).When(x => x.Latitude != null).WithMessage("Latitude deve estar entre -90 e 90 graus.");

        RuleFor(x => x.Longitude)
            .InclusiveBetween(-180, 180).When(x => x.Longitude != null).WithMessage("Longitude deve estar entre -180 e 180 graus.");
    }
}
