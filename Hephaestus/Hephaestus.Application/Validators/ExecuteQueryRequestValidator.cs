using FluentValidation;
using Hephaestus.Domain.DTOs.Request;

namespace Hephaestus.Application.Validators;

public class ExecuteQueryRequestValidator : AbstractValidator<ExecuteQueryRequest>
{
    public ExecuteQueryRequestValidator()
    {
        RuleFor(x => x.Query)
            .NotEmpty().WithMessage("Query � obrigat�ria.")
            .MaximumLength(1000).WithMessage("Query deve ter no m�ximo 1000 caracteres.")
            .Must(q => q.Trim().ToLower().StartsWith("select")).WithMessage("Apenas consultas SELECT s�o permitidas.");
    }
}
