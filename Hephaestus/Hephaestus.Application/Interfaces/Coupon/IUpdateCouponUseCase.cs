using Hephaestus.Domain.DTOs.Request;
using System.Security.Claims;

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
    /// <param name="user">Usuário autenticado.</param>
    Task ExecuteAsync(string id, UpdateCouponRequest request, ClaimsPrincipal user);
}