using FluentValidation;
using Hephaestus.Domain.DTOs.Request;
using System.Collections.Generic;

namespace Hephaestus.Application.Validators;

public class OpenAIChatRequestValidator : AbstractValidator<OpenAIRequest>
{
    public OpenAIChatRequestValidator()
    {
        RuleFor(x => x.Prompt)
            .NotEmpty().WithMessage("Prompt � obrigat�rio.")
            .MaximumLength(1000).WithMessage("Prompt deve ter no m�ximo 1000 caracteres.");

        RuleFor(x => x.Data)
            .MaximumLength(5000).When(x => !string.IsNullOrEmpty(x.Data))
            .WithMessage("Dados devem ter no m�ximo 5000 caracteres.");

        RuleFor(x => x.ResponseFormat)
            .Must(BeValidResponseFormat)
            .When(x => x.ResponseFormat != null)
            .WithMessage("Formato de resposta deve conter a chave 'type' com valor 'text' ou 'json_object'.");
    }

    private bool BeValidResponseFormat(Dictionary<string, string>? responseFormat)
    {
        if (responseFormat == null)
            return true; // permite nulo

        return responseFormat.ContainsKey("type") &&
               (responseFormat["type"] == "text" || responseFormat["type"] == "json_object");
    }
}
