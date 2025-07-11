using FluentValidation;
using Hephaestus.Application.DTOs.Request;
using System.Collections.Generic;

namespace Hephaestus.Application.Validators;

public class OpenAIChatRequestValidator : AbstractValidator<OpenAIChatRequest>
{
    public OpenAIChatRequestValidator()
    {
        RuleFor(x => x.Prompt)
            .NotEmpty().WithMessage("Prompt é obrigatório.")
            .MaximumLength(1000).WithMessage("Prompt deve ter no máximo 1000 caracteres.");

        RuleFor(x => x.Data)
            .MaximumLength(5000).When(x => !string.IsNullOrEmpty(x.Data))
            .WithMessage("Dados devem ter no máximo 5000 caracteres.");

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
