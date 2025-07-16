using Hephaestus.Domain.DTOs.Request;
using System.Security.Claims;

namespace Hephaestus.Application.Interfaces.Auth;

/// <summary>
/// Define o caso de uso para registro de empresas.
/// </summary>
public interface IRegisterCompanyUseCase
{
    /// <summary>
    /// Registra uma nova empresa no sistema.
    /// </summary>
    /// <param name="request">Dados da empresa a ser registrada.</param>
    /// <param name="user">Informa��es do usu�rio autenticado, ou null se n�o autenticado.</param>
    /// <returns>ID da empresa registrada.</returns>
    /// <exception cref="InvalidOperationException">E-mail ou telefone j� registrado.</exception>
    Task<string> ExecuteAsync(RegisterCompanyRequest request, ClaimsPrincipal? user);
}
