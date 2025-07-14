using Hephaestus.Application.DTOs.Response;
using System.Collections.Generic;
using System.Security.Claims;

namespace Hephaestus.Application.Interfaces.Coupon;

/// <summary>
/// Interface para o caso de uso de listagem de cupons.
/// </summary>
public interface IGetCouponsUseCase
{
    /// <summary>
    /// Lista cupons do tenant com filtros opcionais.
    /// </summary>
    /// <param name="user">Usuário autenticado.</param>
    /// <param name="isActive">Filtro opcional para cupons ativos/inativos.</param>
    /// <param name="customerPhoneNumber">Filtro opcional por número de telefone do cliente.</param>
    /// <returns>Lista de cupons.</returns>
    Task<IEnumerable<CouponResponse>> ExecuteAsync(ClaimsPrincipal user, bool? isActive, string? customerPhoneNumber);
}