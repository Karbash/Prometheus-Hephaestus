using Hephaestus.Application.DTOs.Request;
using System.Security.Claims;

namespace Hephaestus.Application.Interfaces.Order;

public interface IUpdateOrderUseCase
{
    Task ExecuteAsync(string id, UpdateOrderRequest request, ClaimsPrincipal user);
}