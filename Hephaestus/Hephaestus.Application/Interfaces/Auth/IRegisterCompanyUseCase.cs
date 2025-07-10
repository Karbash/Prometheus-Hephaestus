using Hephaestus.Application.DTOs.Request;
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
    /// <param name="user">Informações do usuário autenticado, ou null se não autenticado.</param>
    /// <returns>ID da empresa registrada.</returns>
    /// <exception cref="InvalidOperationException">E-mail ou telefone já registrado.</exception>
    Task<string> ExecuteAsync(RegisterCompanyRequest request, ClaimsPrincipal? user);
}