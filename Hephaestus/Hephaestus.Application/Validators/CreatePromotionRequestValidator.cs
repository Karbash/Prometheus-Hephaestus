using FluentValidation;
using Hephaestus.Domain.DTOs.Request;
using Hephaestus.Domain.Enum;

namespace Hephaestus.Application.Validators;

public class CreatePromotionRequestValidator : AbstractValidator<CreatePromotionRequest>
{
    public CreatePromotionRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Nome � obrigat�rio.")
            .MaximumLength(100).WithMessage("Nome n�o pode exceder 100 caracteres.");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Descri��o � obrigat�ria.")
            .MaximumLength(500).WithMessage("Descri��o n�o pode exceder 500 caracteres.");

        RuleFor(x => x.DiscountType)
            .IsInEnum()
            .WithMessage("Tipo de desconto inv�lido. Use: Percentage, Fixed ou FreeItem.");

        RuleFor(x => x.DiscountValue)
            .GreaterThan(0).WithMessage("Valor do desconto deve ser maior que zero.");

        RuleFor(x => x.MenuItemId)
            .NotEmpty()
            .When(x => x.DiscountType == DiscountType.FreeItem)
            .WithMessage("MenuItemId � obrigat�rio para DiscountType FreeItem.");

        RuleFor(x => x.MinOrderValue)
            .GreaterThanOrEqualTo(0).When(x => x.MinOrderValue.HasValue)
            .WithMessage("Valor m�nimo do pedido deve ser maior ou igual a zero.");

        RuleFor(x => x.MaxUsesPerCustomer)
            .GreaterThan(0).When(x => x.MaxUsesPerCustomer.HasValue)
            .WithMessage("M�ximo de usos por cliente deve ser maior que zero.");

        RuleFor(x => x.MaxTotalUses)
            .GreaterThan(0).When(x => x.MaxTotalUses.HasValue)
            .WithMessage("M�ximo de usos totais deve ser maior que zero.");

        RuleFor(x => x.StartDate)
            .NotEmpty().WithMessage("Data de in�cio � obrigat�ria.")
            .LessThanOrEqualTo(x => x.EndDate).WithMessage("Data de in�cio deve ser anterior ou igual � data de t�rmino.");

        RuleFor(x => x.EndDate)
            .NotEmpty().WithMessage("Data de t�rmino � obrigat�ria.")
            .GreaterThanOrEqualTo(x => x.StartDate).WithMessage("Data de t�rmino deve ser posterior ou igual � data de in�cio.");

        RuleFor(x => x.ImageUrl)
            .MaximumLength(500).When(x => !string.IsNullOrEmpty(x.ImageUrl))
            .WithMessage("URL da imagem n�o pode exceder 500 caracteres.");
    }
}
