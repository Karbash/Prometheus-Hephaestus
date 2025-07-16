using FluentValidation;
using Hephaestus.Domain.DTOs.Request;

namespace Hephaestus.Application.Validators;

public class UpdateMenuItemRequestValidator : AbstractValidator<UpdateMenuItemRequest>
{
    public UpdateMenuItemRequestValidator()
    {
        RuleFor(x => x.Name)
            .MaximumLength(100)
            .When(x => !string.IsNullOrEmpty(x.Name))
            .WithMessage("Nome deve ter no máximo 100 caracteres.");

        RuleFor(x => x.Description)
            .MaximumLength(500)
            .When(x => !string.IsNullOrEmpty(x.Description))
            .WithMessage("Descrição deve ter no máximo 500 caracteres.");

        RuleFor(x => x.Price)
            .GreaterThanOrEqualTo(0)
            .When(x => x.Price.HasValue)
            .WithMessage("Preço deve ser maior ou igual a zero.");

        RuleFor(x => x.CategoryId)
            .MaximumLength(36)
            .When(x => !string.IsNullOrEmpty(x.CategoryId))
            .WithMessage("CategoryId deve ter no máximo 36 caracteres.");

        RuleFor(x => x.ImageUrl)
            .MaximumLength(500)
            .When(x => !string.IsNullOrEmpty(x.ImageUrl))
            .WithMessage("URL da imagem deve ter no máximo 500 caracteres.");
    }
}
