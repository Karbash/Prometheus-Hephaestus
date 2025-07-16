using Hephaestus.Domain.DTOs.Response;
using System.Security.Claims;

namespace Hephaestus.Application.Interfaces.Coupon;

/// <summary>
/// Interface para o caso de uso de obten��o de cupom por ID.
/// </summary>
public interface IGetCouponByIdUseCase
{
    /// <summary>
    /// Obt�m detalhes de um cupom por ID.
    /// </summary>
    /// <param name="id">ID do cupom.</param>
    /// <param name="user">Usu�rio autenticado.</param>
    /// <returns>Detalhes do cupom.</returns>
    Task<CouponResponse> ExecuteAsync(string id, ClaimsPrincipal user);
}
