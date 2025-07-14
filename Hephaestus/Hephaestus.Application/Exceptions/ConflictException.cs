namespace Hephaestus.Application.Exceptions;

/// <summary>
/// Exceção lançada quando há conflito de dados (ex: registro duplicado).
/// </summary>
public class ConflictException : ApplicationException
{
    /// <summary>
    /// Tipo do recurso em conflito.
    /// </summary>
    public string ResourceType { get; }

    /// <summary>
    /// Campo que causou o conflito.
    /// </summary>
    public string ConflictingField { get; }

    /// <summary>
    /// Valor que causou o conflito.
    /// </summary>
    public string ConflictingValue { get; }

    /// <summary>
    /// Inicializa uma nova instância da classe <see cref="ConflictException"/>.
    /// </summary>
    /// <param name="message">Mensagem de erro.</param>
    /// <param name="resourceType">Tipo do recurso em conflito.</param>
    /// <param name="conflictingField">Campo que causou o conflito.</param>
    /// <param name="conflictingValue">Valor que causou o conflito.</param>
    public ConflictException(string message, string resourceType, string conflictingField, string conflictingValue)
        : base(message, "CONFLICT")
    {
        ResourceType = resourceType;
        ConflictingField = conflictingField;
        ConflictingValue = conflictingValue;
    }
} 