using System.Runtime.Serialization;

namespace Hephaestus.Application.Exceptions;

/// <summary>
/// Exceção base para a camada de aplicação.
/// </summary>
[Serializable]
public abstract class ApplicationException : Exception
{
    /// <summary>
    /// Código de erro único para identificação do problema.
    /// </summary>
    public string ErrorCode { get; }

    /// <summary>
    /// Detalhes adicionais sobre o erro.
    /// </summary>
    public IDictionary<string, object>? Details { get; }

    /// <summary>
    /// Inicializa uma nova instância da classe <see cref="ApplicationException"/>.
    /// </summary>
    /// <param name="message">Mensagem de erro.</param>
    /// <param name="errorCode">Código de erro único.</param>
    /// <param name="details">Detalhes adicionais sobre o erro.</param>
    /// <param name="innerException">Exceção interna.</param>
    protected ApplicationException(string message, string errorCode, IDictionary<string, object>? details = null, Exception? innerException = null)
        : base(message, innerException)
    {
        ErrorCode = errorCode;
        Details = details;
    }

    /// <summary>
    /// Inicializa uma nova instância da classe <see cref="ApplicationException"/> para serialização.
    /// </summary>
    /// <param name="info">Informações de serialização.</param>
    /// <param name="context">Contexto de streaming.</param>
    [Obsolete("Este construtor é obsoleto. Use o construtor principal.", DiagnosticId = "SYSLIB0051")]
    protected ApplicationException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
        ErrorCode = info.GetString(nameof(ErrorCode)) ?? "UNKNOWN_ERROR";
        Details = null; // Não serializamos Details por simplicidade
    }

    /// <summary>
    /// Define dados de serialização.
    /// </summary>
    /// <param name="info">Informações de serialização.</param>
    /// <param name="context">Contexto de streaming.</param>
    [Obsolete("Este método é obsoleto. Use serialização JSON em vez de serialização binária.", DiagnosticId = "SYSLIB0051")]
    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        base.GetObjectData(info, context);
        info.AddValue(nameof(ErrorCode), ErrorCode);
    }
} 