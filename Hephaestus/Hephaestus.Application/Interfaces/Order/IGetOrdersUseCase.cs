using Hephaestus.Application.DTOs.Response;
using System.Security.Claims;

namespace Hephaestus.Application.Interfaces.Order;

public interface IGetOrdersUseCase
{
    Task<IEnumerable<OrderResponse>> ExecuteAsync(ClaimsPrincipal user, string? customerPhoneNumber, string? status);
}