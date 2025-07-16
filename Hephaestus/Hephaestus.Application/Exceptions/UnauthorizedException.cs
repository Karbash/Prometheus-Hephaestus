namespace Hephaestus.Application.Exceptions;

/// <summary>
/// Exceção lançada quando o usuário não tem autorização para realizar uma ação.
/// </summary>
public class UnauthorizedException : ApplicationException
{
    /// <summary>
    /// Ação que o usuário tentou realizar.
    /// </summary>
    public string Action { get; }

    /// <summary>
    /// Recurso que o usuário tentou acessar.
    /// </summary>
    public string Resource { get; }

    /// <summary>
    /// Inicializa uma nova instância da classe <see cref="UnauthorizedException"/>.
    /// </summary>
    /// <param name="message">Mensagem de erro.</param>
    /// <param name="action">Ação que o usuário tentou realizar.</param>
    /// <param name="resource">Recurso que o usuário tentou acessar.</param>
    public UnauthorizedException(string message, string action, string resource)
        : base(message, "UNAUTHORIZED")
    {
        Action = action;
        Resource = resource;
    }
} 
