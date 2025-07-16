using FluentValidation;
using Hephaestus.Domain.DTOs.Request;

namespace Hephaestus.Application.Validators;

public class CreateOrderRequestValidator : AbstractValidator<CreateOrderRequest>
{
    public CreateOrderRequestValidator()
    {
        RuleFor(x => x.CustomerPhoneNumber)
            .NotEmpty().WithMessage("N�mero de telefone do cliente � obrigat�rio.")
            .MaximumLength(20).WithMessage("N�mero de telefone deve ter no m�ximo 20 caracteres.");

        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("O pedido deve conter pelo menos um item.")
            .Must(items => items.All(i => !string.IsNullOrEmpty(i.MenuItemId)))
            .WithMessage("Todos os itens devem ter um MenuItemId v�lido.");
    }
}
