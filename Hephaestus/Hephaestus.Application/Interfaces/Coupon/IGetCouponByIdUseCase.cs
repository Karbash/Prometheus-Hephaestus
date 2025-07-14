using Hephaestus.Application.DTOs.Response;
using System.Threading.Tasks;

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
    /// <param name="tenantId">ID do tenant extraído do token JWT.</param>
    /// <returns>Detalhes do cupom.</returns>
    Task<CouponResponse> ExecuteAsync(string id, string tenantId);
}