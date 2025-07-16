using FluentValidation;
using Hephaestus.Domain.DTOs.Request;

namespace Hephaestus.Application.Validators;

public class UpdateCouponRequestValidator : AbstractValidator<UpdateCouponRequest>
{
    public UpdateCouponRequestValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("C�digo � obrigat�rio.")
            .MaximumLength(50).WithMessage("C�digo deve ter no m�ximo 50 caracteres.");

        RuleFor(x => x.CustomerPhoneNumber)
            .MaximumLength(20).When(x => !string.IsNullOrEmpty(x.CustomerPhoneNumber))
            .WithMessage("N�mero de telefone deve ter no m�ximo 20 caracteres.");

        RuleFor(x => x.DiscountType)
            .IsInEnum()
            .WithMessage("Tipo de desconto deve ser 'Percentage', 'Fixed' ou 'FreeItem'.");

        RuleFor(x => x.DiscountValue)
            .GreaterThan(0).WithMessage("Valor do desconto deve ser maior que zero.");

        RuleFor(x => x.MenuItemId)
            .Must(BeValidGuid).When(x => !string.IsNullOrEmpty(x.MenuItemId))
            .WithMessage("MenuItemId deve ser um GUID v�lido.");

        RuleFor(x => x.MinOrderValue)
            .GreaterThanOrEqualTo(0).WithMessage("Valor m�nimo do pedido deve ser maior ou igual a zero.");

        RuleFor(x => x.StartDate)
            .NotEmpty().WithMessage("Data de in�cio � obrigat�ria.");

        RuleFor(x => x.EndDate)
            .NotEmpty().WithMessage("Data de t�rmino � obrigat�ria.")
            .GreaterThan(x => x.StartDate).WithMessage("Data de t�rmino deve ser posterior � data de in�cio.");
    }

    private bool BeValidGuid(string? id)
    {
        return id == null || Guid.TryParse(id, out _);
    }
}
