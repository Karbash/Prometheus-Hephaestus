using Hephaestus.Application.DTOs.Response;
using System.Security.Claims;

namespace Hephaestus.Application.Interfaces.Order;

public interface IGetOrderByIdUseCase
{
    Task<OrderResponse> ExecuteAsync(string id, ClaimsPrincipal user);
}