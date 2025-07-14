using Hephaestus.Application.DTOs.Response;
using System.Security.Claims;

namespace Hephaestus.Application.Interfaces.Coupon;

/// <summary>
/// Interface para o caso de uso de obtenção de cupom por ID.
/// </summary>
public interface IGetCouponByIdUseCase
{
    /// <summary>
    /// Obtém detalhes de um cupom por ID.
    /// </summary>
    /// <param name="id">ID do cupom.</param>
    /// <param name="user">Usuário autenticado.</param>
    /// <returns>Detalhes do cupom.</returns>
    Task<CouponResponse> ExecuteAsync(string id, ClaimsPrincipal user);
}