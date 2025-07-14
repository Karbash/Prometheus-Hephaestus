using FluentValidation;
using Hephaestus.Application.DTOs.Request;

namespace Hephaestus.Application.Validators;

public class CreateCouponRequestValidator : AbstractValidator<CreateCouponRequest>
{
    public CreateCouponRequestValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Código é obrigatório.")
            .MaximumLength(50).WithMessage("Código deve ter no máximo 50 caracteres.");

        RuleFor(x => x.CustomerPhoneNumber)
            .MaximumLength(20).When(x => !string.IsNullOrEmpty(x.CustomerPhoneNumber))
            .WithMessage("Número de telefone deve ter no máximo 20 caracteres.");

        RuleFor(x => x.DiscountType)
            .NotEmpty().WithMessage("Tipo de desconto é obrigatório.")
            .Must(x => x == "Percentage" || x == "Fixed" || x == "FreeItem")
            .WithMessage("Tipo de desconto deve ser 'Percentage', 'Fixed' ou 'FreeItem'.");

        RuleFor(x => x.DiscountValue)
            .GreaterThan(0).WithMessage("Valor do desconto deve ser maior que zero.");

        RuleFor(x => x.MenuItemId)
            .Must(BeValidGuid).When(x => !string.IsNullOrEmpty(x.MenuItemId))
            .WithMessage("MenuItemId deve ser um GUID válido.");

        RuleFor(x => x.MinOrderValue)
            .GreaterThanOrEqualTo(0).WithMessage("Valor mínimo do pedido deve ser maior ou igual a zero.");

        RuleFor(x => x.StartDate)
            .NotEmpty().WithMessage("Data de início é obrigatória.");

        RuleFor(x => x.EndDate)
            .NotEmpty().WithMessage("Data de término é obrigatória.")
            .GreaterThan(x => x.StartDate).WithMessage("Data de término deve ser posterior à data de início.");
    }

    private bool BeValidGuid(string? id)
    {
        return id == null || Guid.TryParse(id, out _);
    }
}