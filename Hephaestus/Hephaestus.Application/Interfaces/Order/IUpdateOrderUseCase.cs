using Hephaestus.Domain.DTOs.Request;
using System.Security.Claims;

namespace Hephaestus.Application.Interfaces.Order;

public interface IUpdateOrderUseCase
{
    Task ExecuteAsync(UpdateOrderRequest request, ClaimsPrincipal user);
    Task ExecutePartialAsync(UpdateOrderRequest request, System.Security.Claims.ClaimsPrincipal user);
}
