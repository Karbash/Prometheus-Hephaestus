using Hephaestus.Application.DTOs.Request;
using System.Security.Claims;

namespace Hephaestus.Application.Interfaces.Coupon;

/// <summary>
/// Interface para o caso de uso de criação de cupons.
/// </summary>
public interface ICreateCouponUseCase
{
    /// <summary>
    /// Cria um novo cupom para o tenant.
    /// </summary>
    /// <param name="request">Dados do cupom a ser criado.</param>
    /// <param name="user">Usuário autenticado.</param>
    /// <returns>ID do cupom criado.</returns>
    Task<string> ExecuteAsync(CreateCouponRequest request, ClaimsPrincipal user);
}