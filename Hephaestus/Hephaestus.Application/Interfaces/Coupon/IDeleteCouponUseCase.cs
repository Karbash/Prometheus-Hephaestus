using System.Threading.Tasks;
using System.Security.Claims;

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
    /// <param name="user">Usuário autenticado.</param>
    Task ExecuteAsync(string id, ClaimsPrincipal user);
}
