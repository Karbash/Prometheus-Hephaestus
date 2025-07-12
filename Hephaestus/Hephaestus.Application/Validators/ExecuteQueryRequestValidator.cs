using FluentValidation;
using Hephaestus.Application.DTOs.Request;

namespace Hephaestus.Application.Validators;

public class ExecuteQueryRequestValidator : AbstractValidator<ExecuteQueryRequest>
{
    public ExecuteQueryRequestValidator()
    {
        RuleFor(x => x.Query)
            .NotEmpty().WithMessage("Query é obrigatória.")
            .MaximumLength(1000).WithMessage("Query deve ter no máximo 1000 caracteres.")
            .Must(q => q.Trim().ToLower().StartsWith("select")).WithMessage("Apenas consultas SELECT são permitidas.");
    }
}