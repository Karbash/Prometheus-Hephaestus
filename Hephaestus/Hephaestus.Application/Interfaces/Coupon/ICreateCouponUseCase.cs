using Hephaestus.Domain.DTOs.Request;
using System.Security.Claims;

namespace Hephaestus.Application.Interfaces.Coupon;

/// <summary>
/// Interface para o caso de uso de cria��o de cupons.
/// </summary>
public interface ICreateCouponUseCase
{
    /// <summary>
    /// Cria um novo cupom para o tenant.
    /// </summary>
    /// <param name="request">Dados do cupom a ser criado.</param>
    /// <param name="user">Usu�rio autenticado.</param>
    /// <returns>ID do cupom criado.</returns>
    Task<string> ExecuteAsync(CreateCouponRequest request, ClaimsPrincipal user);
}
