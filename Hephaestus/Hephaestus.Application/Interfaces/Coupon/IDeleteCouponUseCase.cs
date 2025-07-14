using System.Threading.Tasks;

namespace Hephaestus.Application.Interfaces.Coupon;

/// <summary>
/// Interface para o caso de uso de remoção de cupons.
/// </summary>
public interface IDeleteCouponUseCase
{
    /// <summary>
    /// Remove um cupom do tenant.
    /// </summary>
    /// <param name="id">ID do cupom a ser removido.</param>
    /// <param name="tenantId">ID do tenant extraído do token JWT.</param>
    Task ExecuteAsync(string id, string tenantId);
}