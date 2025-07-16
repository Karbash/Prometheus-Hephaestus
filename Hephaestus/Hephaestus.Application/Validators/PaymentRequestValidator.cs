using FluentValidation;
using Hephaestus.Domain.DTOs.Request;

namespace Hephaestus.Application.Validators;

public class PaymentRequestValidator : AbstractValidator<PaymentRequest>
{
    public PaymentRequestValidator()
    {
        RuleFor(x => x.Amount)
            .GreaterThan(0)
            .WithMessage("O valor do pagamento deve ser maior que zero.");
        RuleFor(x => x.Currency)
            .NotEmpty()
            .WithMessage("A moeda � obrigat�ria.");
        RuleFor(x => x.PaymentMethod)
            .NotEmpty()
            .WithMessage("O m�todo de pagamento � obrigat�rio.");
        RuleFor(x => x.CustomerId)
            .NotEmpty()
            .WithMessage("O identificador do cliente � obrigat�rio.");
        RuleFor(x => x.OrderId)
            .NotEmpty()
            .WithMessage("O identificador do pedido � obrigat�rio.");
    }
}
