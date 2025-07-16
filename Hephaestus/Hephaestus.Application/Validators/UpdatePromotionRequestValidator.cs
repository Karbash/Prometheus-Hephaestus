using FluentValidation;
using Hephaestus.Domain.DTOs.Request;

namespace Hephaestus.Application.Validators;

public class UpdatePromotionRequestValidator : AbstractValidator<UpdatePromotionRequest>
{
    public UpdatePromotionRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Nome � obrigat�rio.")
            .MaximumLength(100)
            .WithMessage("Nome deve ter no m�ximo 100 caracteres.");

        RuleFor(x => x.Description)
            .MaximumLength(500)
            .When(x => !string.IsNullOrEmpty(x.Description))
            .WithMessage("Descri��o deve ter no m�ximo 500 caracteres.");

        RuleFor(x => x.DiscountType)
            .NotEmpty()
            .WithMessage("Tipo de desconto � obrigat�rio.")
            .IsInEnum()
            .WithMessage("Tipo de desconto inv�lido.");

        RuleFor(x => x.DiscountValue)
            .GreaterThan(0)
            .WithMessage("Valor do desconto deve ser maior que zero.");

        RuleFor(x => x.MinOrderValue)
            .GreaterThanOrEqualTo(0)
            .When(x => x.MinOrderValue.HasValue)
            .WithMessage("Valor m�nimo do pedido deve ser maior ou igual a zero.");

        RuleFor(x => x.MaxUsesPerCustomer)
            .GreaterThan(0)
            .When(x => x.MaxUsesPerCustomer.HasValue)
            .WithMessage("Uso m�ximo por cliente deve ser maior que zero.");

        RuleFor(x => x.MaxTotalUses)
            .GreaterThan(0)
            .When(x => x.MaxTotalUses.HasValue)
            .WithMessage("Uso total m�ximo deve ser maior que zero.");

        RuleFor(x => x.StartDate)
            .NotEmpty()
            .WithMessage("Data de in�cio � obrigat�ria.");

        RuleFor(x => x.EndDate)
            .NotEmpty()
            .WithMessage("Data de fim � obrigat�ria.")
            .GreaterThan(x => x.StartDate)
            .WithMessage("Data de fim deve ser posterior � data de in�cio.");
    }
}
