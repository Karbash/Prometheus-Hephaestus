using FluentValidation;
using Hephaestus.Application.DTOs.Request;

namespace Hephaestus.Application.Validators;

/// <summary>
/// Validador para DTOs de criação e atualização de itens do cardápio.
/// </summary>
public class CreateMenuItemRequestValidator : AbstractValidator<CreateMenuItemRequest>
{
    public CreateMenuItemRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Nome é obrigatório.")
            .MaximumLength(100).WithMessage("Nome deve ter no máximo 100 caracteres.");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Descrição é obrigatória.")
            .MaximumLength(500).WithMessage("Descrição deve ter no máximo 500 caracteres.");

        RuleFor(x => x.CategoryId)
            .NotEmpty().WithMessage("CategoryId é obrigatório.")
            .Must(BeValidGuid).WithMessage("CategoryId deve ser um GUID válido.");

        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("Preço deve ser maior que zero.");

        RuleFor(x => x.Tags)
            .NotNull().WithMessage("Tags não podem ser nulas.")
            .ForEach(tag => tag.MaximumLength(50).WithMessage("Cada tag deve ter no máximo 50 caracteres."));

        RuleFor(x => x.AvailableAdditionalIds)
            .NotNull().WithMessage("AvailableAdditionalIds não pode ser nulo.")
            .ForEach(id => id.Must(BeValidGuid).WithMessage("Cada ID de adicional deve ser um GUID válido."));

        RuleFor(x => x.ImageUrl)
            .MaximumLength(500).When(x => x.ImageUrl != null).WithMessage("URL da imagem deve ter no máximo 500 caracteres.")
            .Must(BeValidUrl).When(x => x.ImageUrl != null).WithMessage("URL da imagem inválida.");
    }

    private bool BeValidGuid(string id)
    {
        return Guid.TryParse(id, out _);
    }

    private bool BeValidUrl(string? url)
    {
        return Uri.TryCreate(url, UriKind.Absolute, out _);
    }
}

