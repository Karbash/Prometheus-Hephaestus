namespace Hephaestus.Application.Exceptions;

/// <summary>
/// Exceção lançada quando um recurso não é encontrado.
/// </summary>
public class NotFoundException : ApplicationException
{
    /// <summary>
    /// Tipo do recurso não encontrado.
    /// </summary>
    public string ResourceType { get; }

    /// <summary>
    /// Identificador do recurso não encontrado.
    /// </summary>
    public string ResourceId { get; }

    /// <summary>
    /// Inicializa uma nova instância da classe <see cref="NotFoundException"/>.
    /// </summary>
    /// <param name="resourceType">Tipo do recurso não encontrado.</param>
    /// <param name="resourceId">Identificador do recurso não encontrado.</param>
    public NotFoundException(string resourceType, string resourceId)
        : base($"{resourceType} com ID '{resourceId}' não encontrado.", "RESOURCE_NOT_FOUND")
    {
        ResourceType = resourceType;
        ResourceId = resourceId;
    }

    /// <summary>
    /// Inicializa uma nova instância da classe <see cref="NotFoundException"/>.
    /// </summary>
    /// <param name="message">Mensagem de erro.</param>
    /// <param name="resourceType">Tipo do recurso não encontrado.</param>
    /// <param name="resourceId">Identificador do recurso não encontrado.</param>
    public NotFoundException(string message, string resourceType, string resourceId)
        : base(message, "RESOURCE_NOT_FOUND")
    {
        ResourceType = resourceType;
        ResourceId = resourceId;
    }
} 
