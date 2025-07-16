namespace Hephaestus.Application.Exceptions;

/// <summary>
/// Exceção lançada quando uma regra de negócio é violada.
/// </summary>
public class BusinessRuleException : ApplicationException
{
    /// <summary>
    /// Nome da regra de negócio violada.
    /// </summary>
    public string RuleName { get; }

    /// <summary>
    /// Inicializa uma nova instância da classe <see cref="BusinessRuleException"/>.
    /// </summary>
    /// <param name="message">Mensagem de erro.</param>
    /// <param name="ruleName">Nome da regra de negócio violada.</param>
    public BusinessRuleException(string message, string ruleName)
        : base(message, "BUSINESS_RULE_VIOLATION")
    {
        RuleName = ruleName;
    }

    /// <summary>
    /// Inicializa uma nova instância da classe <see cref="BusinessRuleException"/>.
    /// </summary>
    /// <param name="message">Mensagem de erro.</param>
    /// <param name="ruleName">Nome da regra de negócio violada.</param>
    /// <param name="details">Detalhes adicionais sobre o erro.</param>
    public BusinessRuleException(string message, string ruleName, IDictionary<string, object>? details)
        : base(message, "BUSINESS_RULE_VIOLATION", details)
    {
        RuleName = ruleName;
    }
} 
