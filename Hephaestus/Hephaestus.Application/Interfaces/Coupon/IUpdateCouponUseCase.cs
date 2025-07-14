using Hephaestus.Application.DTOs.Request;
using System.Threading.Tasks;

namespace Hephaestus.Application.Interfaces.Coupon;

/// <summary>
/// Interface para o caso de uso de atualização de cupons.
/// </summary>
public interface IUpdateCouponUseCase
{
    /// <summary>
    /// Atualiza um cupom existente.
    /// </summary>
    /// <param name="id">ID do cupom a ser atualizado.</param>
    /// <param name="request">Dados atualizados do cupom.</param>
    /// <param name="tenantId">ID do tenant extraído do token JWT.</param>
    Task ExecuteAsync(string id, UpdateCouponRequest request, string tenantId);
}