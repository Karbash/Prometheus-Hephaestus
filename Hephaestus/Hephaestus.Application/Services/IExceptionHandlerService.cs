using Hephaestus.Application.Exceptions;

namespace Hephaestus.Application.Services;

/// <summary>
/// Serviço para tratamento centralizado de exceções.
/// </summary>
public interface IExceptionHandlerService
{
    /// <summary>
    /// Trata uma exceção e retorna informações estruturadas sobre o erro.
    /// </summary>
    /// <param name="exception">Exceção a ser tratada.</param>
    /// <returns>Informações estruturadas sobre o erro.</returns>
    ExceptionInfo HandleException(Exception exception);

    /// <summary>
    /// Verifica se uma exceção é tratável pelo serviço.
    /// </summary>
    /// <param name="exception">Exceção a ser verificada.</param>
    /// <returns>True se a exceção pode ser tratada.</returns>
    bool CanHandle(Exception exception);
}

/// <summary>
/// Informações estruturadas sobre uma exceção.
/// </summary>
public record ExceptionInfo
{
    /// <summary>
    /// Código de erro único.
    /// </summary>
    public required string ErrorCode { get; init; }

    /// <summary>
    /// Mensagem de erro para o usuário.
    /// </summary>
    public required string Message { get; init; }

    /// <summary>
    /// Código de status HTTP apropriado.
    /// </summary>
    public required int StatusCode { get; init; }

    /// <summary>
    /// Detalhes adicionais sobre o erro.
    /// </summary>
    public IDictionary<string, object>? Details { get; init; }

    /// <summary>
    /// Tipo de erro para categorização.
    /// </summary>
    public required string ErrorType { get; init; }
} 