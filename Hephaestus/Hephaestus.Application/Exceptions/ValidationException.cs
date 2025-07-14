using FluentValidation.Results;

namespace Hephaestus.Application.Exceptions;

/// <summary>
/// Exceção lançada quando há erros de validação.
/// </summary>
public class ValidationException : ApplicationException
{
    /// <summary>
    /// Resultados da validação do FluentValidation.
    /// </summary>
    public ValidationResult ValidationResult { get; }

    /// <summary>
    /// Inicializa uma nova instância da classe <see cref="ValidationException"/>.
    /// </summary>
    /// <param name="validationResult">Resultado da validação.</param>
    public ValidationException(ValidationResult validationResult)
        : base("Erro de validação", "VALIDATION_ERROR")
    {
        ValidationResult = validationResult;
    }

    /// <summary>
    /// Inicializa uma nova instância da classe <see cref="ValidationException"/>.
    /// </summary>
    /// <param name="message">Mensagem de erro.</param>
    /// <param name="validationResult">Resultado da validação.</param>
    public ValidationException(string message, ValidationResult validationResult)
        : base(message, "VALIDATION_ERROR")
    {
        ValidationResult = validationResult;
    }
} 