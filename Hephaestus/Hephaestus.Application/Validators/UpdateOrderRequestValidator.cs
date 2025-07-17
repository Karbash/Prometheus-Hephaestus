using FluentValidation;
using Hephaestus.Domain.DTOs.Request;

namespace Hephaestus.Application.Validators;

public class UpdateOrderRequestValidator : AbstractValidator<UpdateOrderRequest>
{
    public UpdateOrderRequestValidator()
    {
        RuleFor(x => x.CustomerPhoneNumber)
            .NotEmpty().WithMessage("N�mero de telefone do cliente � obrigat�rio.")
            .MaximumLength(20).WithMessage("N�mero de telefone deve ter no m�ximo 20 caracteres.");

        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("O pedido deve conter pelo menos um item.")
            .Must(items => items.All(i => !string.IsNullOrEmpty(i.MenuItemId)))
            .WithMessage("Todos os itens devem ter um MenuItemId v�lido.");

        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.AdditionalIds)
                .Must(list => list == null || list.All(id => !string.IsNullOrWhiteSpace(id)))
                .WithMessage("Todos os AdditionalIds devem ser válidos.");
            item.RuleFor(i => i.TagIds)
                .Must(list => list == null || list.All(id => !string.IsNullOrWhiteSpace(id)))
                .WithMessage("Todos os TagIds devem ser válidos.");
        });

        RuleFor(x => x.Status)
            .IsInEnum().WithMessage("Status do pedido inv�lido.");

        RuleFor(x => x.PaymentStatus)
            .IsInEnum().WithMessage("Status de pagamento inv�lido.");

        When(x => x.Address != null, () =>
        {
            RuleFor(x => x.Address.Street)
                .NotEmpty().WithMessage("Rua de entrega é obrigatória.")
                .MaximumLength(200).WithMessage("Rua de entrega deve ter no máximo 200 caracteres.");
            RuleFor(x => x.Address.Number)
                .NotEmpty().WithMessage("Número de entrega é obrigatório.")
                .MaximumLength(20).WithMessage("Número de entrega deve ter no máximo 20 caracteres.");
            RuleFor(x => x.Address.City)
                .NotEmpty().WithMessage("Cidade de entrega é obrigatória.")
                .MaximumLength(100).WithMessage("Cidade de entrega deve ter no máximo 100 caracteres.");
            RuleFor(x => x.Address.State)
                .NotEmpty().WithMessage("Estado de entrega é obrigatório.")
                .MaximumLength(2).WithMessage("Estado de entrega deve ter no máximo 2 caracteres.");
            RuleFor(x => x.Address.ZipCode)
                .MaximumLength(20).When(x => !string.IsNullOrEmpty(x.Address.ZipCode)).WithMessage("CEP de entrega deve ter no máximo 20 caracteres.");
        });
    }
}
