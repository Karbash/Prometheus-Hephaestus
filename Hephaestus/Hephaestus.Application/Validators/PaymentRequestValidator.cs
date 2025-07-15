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
            .WithMessage("A moeda é obrigatória.");
        RuleFor(x => x.PaymentMethod)
            .NotEmpty()
            .WithMessage("O método de pagamento é obrigatório.");
        RuleFor(x => x.CustomerId)
            .NotEmpty()
            .WithMessage("O identificador do cliente é obrigatório.");
        RuleFor(x => x.OrderId)
            .NotEmpty()
            .WithMessage("O identificador do pedido é obrigatório.");
    }
}