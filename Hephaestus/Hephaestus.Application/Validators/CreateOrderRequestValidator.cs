using FluentValidation;
using Hephaestus.Domain.DTOs.Request;

namespace Hephaestus.Application.Validators;

public class CreateOrderRequestValidator : AbstractValidator<CreateOrderRequest>
{
    public CreateOrderRequestValidator()
    {
        RuleFor(x => x.CustomerPhoneNumber)
            .NotEmpty().WithMessage("Número de telefone do cliente é obrigatório.")
            .MaximumLength(20).WithMessage("Número de telefone deve ter no máximo 20 caracteres.");

        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("O pedido deve conter pelo menos um item.")
            .Must(items => items.All(i => !string.IsNullOrEmpty(i.MenuItemId)))
            .WithMessage("Todos os itens devem ter um MenuItemId válido.");
    }
}
