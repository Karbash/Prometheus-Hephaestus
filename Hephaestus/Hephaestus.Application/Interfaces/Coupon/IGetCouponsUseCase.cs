using Hephaestus.Application.DTOs.Response;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Hephaestus.Application.Interfaces.Coupon;

/// <summary>
/// Interface para o caso de uso de listagem de cupons.
/// </summary>
public interface IGetCouponsUseCase
{
    /// <summary>
    /// Lista cupons do tenant com filtros opcionais.
    /// </summary>
    /// <param name="tenantId">ID do tenant extraído do token JWT.</param>
    /// <param name="isActive">Filtro opcional para cupons ativos/inativos.</param>
    /// <param name="customerPhoneNumber">Filtro opcional por número de telefone do cliente.</param>
    /// <returns>Lista de cupons.</returns>
    Task<IEnumerable<CouponResponse>> ExecuteAsync(string tenantId, bool? isActive, string? customerPhoneNumber);
}