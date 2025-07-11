using FluentValidation;
using Hephaestus.Application.DTOs.Request;

namespace Hephaestus.Application.Validators
{
    public class UpdateMenuItemRequestValidator : AbstractValidator<UpdateMenuItemRequest>
    {
        public UpdateMenuItemRequestValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("ID é obrigatório.")
                .Must(BeValidGuid).WithMessage("ID deve ser um GUID válido.");

            RuleFor(x => x.Name)
                .MaximumLength(100).When(x => x.Name != null).WithMessage("Nome deve ter no máximo 100 caracteres.");

            RuleFor(x => x.Description)
                .MaximumLength(500).When(x => x.Description != null).WithMessage("Descrição deve ter no máximo 500 caracteres.");

            RuleFor(x => x.CategoryId)
                .Must(BeValidGuid).When(x => x.CategoryId != null).WithMessage("CategoryId deve ser um GUID válido.");

            RuleFor(x => x.Price)
                .GreaterThan(0).When(x => x.Price != null).WithMessage("Preço deve ser maior que zero.");

            RuleFor(x => x.Tags)
                .ForEach(tag => tag.MaximumLength(50).WithMessage("Cada tag deve ter no máximo 50 caracteres.")).When(x => x.Tags != null);

            RuleFor(x => x.AvailableAdditionalIds)
                .ForEach(id => id.Must(BeValidGuid).WithMessage("Cada ID de adicional deve ser um GUID válido.")).When(x => x.AvailableAdditionalIds != null);

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
}
