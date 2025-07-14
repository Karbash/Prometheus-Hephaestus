using Hephaestus.Application.DTOs.Request;
using System.Threading.Tasks;

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
    /// <param name="tenantId">ID do tenant extraído do token JWT.</param>
    /// <returns>ID do cupom criado.</returns>
    Task<string> ExecuteAsync(CreateCouponRequest request, string tenantId);
}